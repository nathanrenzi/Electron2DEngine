using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using Electron2D.Core.ECS;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace Electron2D.Core
{
    public class RigidbodySystem : BaseSystem<Rigidbody> { }
    public class Rigidbody : Component
    {
        public static readonly float Epsilon = 1f;

        public uint ID { get; private set; } = uint.MaxValue;
        public Vector2 Velocity
        {
            get
            {
                return Physics.GetBodyVelocity(ID);
            }
            set
            {
                Physics.SetLinearVelocity(ID, value);
            }
        }
        public float AngularVelocity
        {
            get
            {
                return Physics.GetBodyAngularVelocity(ID);
            }
            set
            {
                Physics.SetAngularVelocity(ID, value);
            }
        }
        public RigidbodyMode Mode { get; private set; }
        public RigidbodyShape Shape { get; private set; }

        // https://www.iforce2d.net/b2dtut/collision-filtering
        /// <summary>
        /// The layer of this rigidbody determines how other rigidbodies will interact with this one.
        /// </summary>
        public ushort Layer { get; private set; }
        /// <summary>
        /// The hit mask determines what layers this rigidbody can collide with.
        /// </summary>
        public ushort HitMask { get; private set; }
        /// <summary>
        /// If either rigidbody has a GroupIndex of zero, use the category/mask rules as above.
        /// If both GroupIndex values are non-zero but different, use the category/mask rules as above.
        /// If both GroupIndex values are the same and positive, collide.
        /// If both GroupIndex values are the same and negative, don't collide.
        /// </summary>
        public short GroupIndex { get; private set; }

        // Starting Values
        private Vector2 velocity;
        private float angularVelocity;
        private float density;
        private float friction;
        private float mass;
        private bool fixedRotation;
        // ---------------

        private Vector2 newPosition;
        private float newAngle;
        private Vector2 oldPosition;
        private float oldAngle;
        private float lerpDeltaTime;
        private float lastLerpTime;

        private Transform transform;

        private bool valid = false;
        private bool interpolationReady = false;

        /// <summary>
        /// Creates a dynamic rigidbody.
        /// </summary>
        /// <param name="_startVelocity"></param>
        /// <param name="_startAngularVelocity"></param>
        /// <param name="_density"></param>
        /// <param name="_friction"></param>
        /// <param name="_mass"></param>
        public Rigidbody(Vector2 _startVelocity, float _startAngularVelocity, float _friction, float _density = 1, float _mass = 0, bool _fixedRotation = false,
            RigidbodyShape _rigidbodyShape = RigidbodyShape.Box, RigidbodyMode _rigidbodyMode = RigidbodyMode.AutoMass,
            ushort _layer = 0x0001, ushort _hitMask = 0xFFFF, short _groupIndex = 0)
        {
            velocity = _startVelocity;
            angularVelocity = _startAngularVelocity;
            friction = _friction;
            density = _density;
            mass = _mass;
            fixedRotation = _fixedRotation;
            Mode = _rigidbodyMode;
            Shape = _rigidbodyShape;
            Layer = _layer;
            HitMask = _hitMask;
            GroupIndex = _groupIndex;

            RigidbodySystem.Register(this);
        }

        /// <summary>
        /// Creates a static rigidbody.
        /// </summary>
        public Rigidbody(float _friction = 1, RigidbodyShape _rigidbodyShape = RigidbodyShape.Box,
            ushort _layer = 0x0001, ushort _hitMask = 0xFFFF, short _groupIndex = 0)
        {
            density = 1;
            friction = _friction;
            Mode = RigidbodyMode.StaticMassless; // Static Rigidbody
            Shape = _rigidbodyShape;
            Layer = _layer;
            HitMask = _hitMask;
            GroupIndex = _groupIndex;

            RigidbodySystem.Register(this);
        }

        ~Rigidbody()
        {
            RigidbodySystem.Unregister(this);
            if(ID != uint.MaxValue) Physics.RemovePhysicsBody(ID);
        }

        public override void OnAdded()
        {
            transform = GetComponent<Transform>();
            if (transform == null)
            {
                Debug.LogError("PHYSICS: A rigidbody is trying to be added to an entity without a Transform component, removing collider...");
                Entity.RemoveComponent(this);
                valid = false;
                Dispose();
                return;
            }

            BodyDef bodyDef = new BodyDef()
            {
                Position = new Vec2(transform.Position.X / Physics.WorldScalar, transform.Position.Y / Physics.WorldScalar),
                Angle = transform.Rotation,
                LinearVelocity = new Vec2(velocity.X, velocity.Y),
                AngularVelocity = angularVelocity,
                FixedRotation = fixedRotation
            };

            FixtureDef fixtureDef;
            if(Shape == RigidbodyShape.Box)
            {
                PolygonDef polygonDef = new PolygonDef()
                {
                    Density = density,
                    Friction = friction,
                    Filter = new FilterData()
                    {
                        CategoryBits = Layer,
                        MaskBits = HitMask,
                        GroupIndex = GroupIndex
                    }
                };
                polygonDef.SetAsBox((transform.Scale.X - Epsilon) / 2f / Physics.WorldScalar, (transform.Scale.Y - Epsilon) / 2f / Physics.WorldScalar);
                fixtureDef = polygonDef;
            }
            else if(Shape == RigidbodyShape.Circle)
            {
                CircleDef circleDef = new CircleDef()
                {
                    Density = density,
                    Friction = friction,
                    Radius = (transform.Scale.X - Epsilon) / 2f / Physics.WorldScalar,
                    Filter = new FilterData()
                    {
                        CategoryBits = Layer,
                        MaskBits = HitMask,
                        GroupIndex = GroupIndex
                    }
                };
                fixtureDef = circleDef;
            }
            else
            {
                // Should only get this error when developing
                Debug.LogError($"Unsupported rigidbody shape {Shape}.");
                return;
            }
            
            if (Mode == RigidbodyMode.AutoMass)
            {
                // Set Mass Automatically
                ID = Physics.CreatePhysicsBody(bodyDef, fixtureDef, true);
                valid = true;
            }
            else if(Mode == RigidbodyMode.ManualMass)
            {
                // Set Mass Manually
                ID = Physics.CreatePhysicsBody(bodyDef, fixtureDef, new MassData() { Mass = mass });
                valid = true;
            }
            else if(Mode == RigidbodyMode.StaticMassless)
            {
                // Static Rigidbody
                ID = Physics.CreatePhysicsBody(bodyDef, fixtureDef, false);
                valid = true;
            }
        }

        public override void FixedUpdate()
        {
            if (!valid || ID == uint.MaxValue) return;

            // Setting values for interpolation
            oldPosition = transform.Position / Physics.WorldScalar;
            oldAngle = transform.Rotation;

            newPosition = Physics.GetBodyPosition(ID);
            newAngle = Physics.GetBodyRotation(ID);

            lerpDeltaTime = Time.FixedDeltaTime;
            lastLerpTime = (float)GLFW.Glfw.Time;

            interpolationReady = true;
        }

        public override void Update()
        {
            if (!valid || ID == uint.MaxValue || !interpolationReady) return;

            // Interpolation
            float t = MathEx.Clamp01((Time.TotalElapsedSeconds - lastLerpTime) / lerpDeltaTime);

            //      Position
            transform.Position = Vector2.Lerp(oldPosition, newPosition, t) * Physics.WorldScalar;

            //      Rotation
            transform.Rotation = (float)(oldAngle * (1.0 - t) + (newAngle * t));
        }
    }

    public enum RigidbodyShape
    {
        Box,
        Circle,
        Mesh
    }

    public enum RigidbodyMode
    {
        AutoMass,
        ManualMass,
        StaticMassless
    }
}
