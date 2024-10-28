using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.Joints;
using Electron2D.Core.ECS;
using System.Numerics;

namespace Electron2D.Core.PhysicsBox2D
{
    public class RigidbodySystem : BaseSystem<Rigidbody> { }
    public class Rigidbody : Component
    {
        public static readonly float Epsilon = 1f;

        public Action<Rigidbody> OnBeginContact { get; set; }
        public Action<Rigidbody> OnEndContact { get; set; }
        public List<Rigidbody> CurrentContacts { get; set; } = new List<Rigidbody>();
        public List<uint> Joints { get; private set; } = new List<uint>();
        public bool IsDestroyed { get; private set; }
        public uint ID { get; private set; } = uint.MaxValue;
        public Vector2 Velocity
        {
            get
            {
                return Physics.GetBodyVelocity(ID);
            }
            set
            {
                Physics.SetVelocity(ID, value);
            }
        }
        /// <summary>
        /// Instead of getting the velocity from the physics simulation, it is calculated using the last frame's
        /// and this frame's position. This can help when the rigidbody is behaving strangely and the velocity is
        /// not accurate.
        /// </summary>
        public Vector2 CalculatedVelocity
        {
            get
            {
                return newPosition - oldPosition;
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
        public bool IsStatic { get; private set; }

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
        /// <summary>
        /// Vertex array for a custom convex collider. Only used when <see cref="Shape"/> is set to <see cref="RigidbodyShape.ConvexMesh"/>
        /// </summary>
        public Vector2[] ConvexColliderPoints { get; private set; }

        // Starting Values
        private Vector2 velocity;
        private float angularVelocity;
        private float linearDampening;
        private float angularDampening;
        private float density;
        private float friction;
        private float bounciness;
        private MassData massData;
        private bool fixedRotation;
        // ---------------

        private Vector2 newPosition;
        private float newAngle;
        private Vector2 oldPosition;
        private float oldAngle;
        private float lerpDeltaTime;
        private float lastLerpTime;

        private Transform transform;

        private bool isValid = false;
        private bool interpolationReady = false;

        public static Rigidbody CreateDynamic(RigidbodyDynamicDef _definition)
        {
            return new Rigidbody(false, _definition.Velocity, _definition.AngularVelocity, _definition.MassData, _definition.Friction, _definition.Bounciness,
                _definition.Density, _definition.LinearDampening, _definition.AngularDampening, _definition.FixedRotation, _definition.Shape,
                RigidbodyMode.Dynamic, _definition.Layer, _definition.HitMask, _definition.GroupIndex, _definition.ConvexColliderPoints);
        }

        public static Rigidbody CreateKinematic(RigidbodyStaticDef _definition)
        {
            return new Rigidbody(true, new Vector2(0, 0), 0, new MassData(), _definition.Friction, _definition.Bounciness, 1, 0.0f, 0.0f, false, _definition.Shape,
                RigidbodyMode.Kinematic, _definition.Layer, _definition.HitMask, _definition.GroupIndex, _definition.ConvexColliderPoints);
        }

        private Rigidbody(bool _isStatic, Vector2 _startVelocity, float _startAngularVelocity, MassData _massData, float _friction, float _bounciness,
            float _density, float _linearDampening, float _angularDampening, bool _fixedRotation, RigidbodyShape _rigidbodyShape,
            RigidbodyMode _rigidbodyMode, ushort _layer, ushort _hitMask, short _groupIndex, Vector2[] _convexColliderPoints)
        {
            IsStatic = _isStatic;
            velocity = _startVelocity;
            angularVelocity = _startAngularVelocity;
            linearDampening = _linearDampening;
            angularDampening = _angularDampening;
            friction = _friction;
            bounciness = _bounciness;
            density = _density;
            massData = _massData;
            fixedRotation = _fixedRotation;
            Mode = _rigidbodyMode;
            Shape = _rigidbodyShape;
            Layer = _layer;
            HitMask = _hitMask;
            GroupIndex = _groupIndex;
            ConvexColliderPoints = _convexColliderPoints;

            RigidbodySystem.Register(this);
        }

        protected override void OnDispose()
        {
            if (IsDestroyed) return;
            RigidbodySystem.Unregister(this);
            if(ID != uint.MaxValue) Physics.RemovePhysicsBody(ID);
            IsDestroyed = true;
        }

        public static void InvokeCollision(uint _id, uint _hitId, bool _beginContact)
        {
            List<Rigidbody> rigidbodies = RigidbodySystem.GetComponents();

            Rigidbody body1 = null;
            Rigidbody body2 = null;
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                if (rigidbodies[i].ID == _id)
                {
                    body1 = rigidbodies[i];
                }

                if (rigidbodies[i].ID == _hitId)
                {
                    body2 = rigidbodies[i];
                }
            }
            if (body1 == null) return;
            if (body2 == null) return;

            if (_beginContact)
            {
                // Begin Contact
                body1.OnBeginContact?.Invoke(body2);
                body1.CurrentContacts.Add(body2);
            }
            else
            {
                // End Contact
                body1.OnEndContact?.Invoke(body1);
                body1.CurrentContacts.Remove(body1);
            }
        }

        public override void OnAdded()
        {
            transform = GetComponent<Transform>();
            if (transform == null)
            {
                Debug.LogError("PHYSICS: A rigidbody is trying to be added to an entity without a Transform component, removing...");
                Entity.RemoveComponent(this);
                isValid = false;
                Dispose();
                return;
            }

            BodyDef bodyDef = new BodyDef()
            {
                position = transform.Position / Physics.WorldScalar,
                angle = transform.Rotation,
                linearVelocity = velocity,
                angularVelocity = angularVelocity,
                linearDamping = linearDampening,
                angularDamping = angularDampening,
                fixedRotation = fixedRotation
            };

            FixtureDef fixtureDef = new FixtureDef()
            {
                density = density,
                friction = friction,
                restitution = bounciness,
                filter = new Filter()
                {
                    categoryBits = Layer,
                    maskBits = HitMask,
                    groupIndex = GroupIndex
                }
            };
            switch(this.Shape)
            {
                case RigidbodyShape.Box:
                    fixtureDef.shape = new PolygonShape((transform.Scale.X - Epsilon) / 2f / Physics.WorldScalar, (transform.Scale.Y - Epsilon) / 2f / Physics.WorldScalar);
                    break;
                case RigidbodyShape.Circle:
                    fixtureDef.shape = new CircleShape()
                    {
                        Radius = (transform.Scale.X - Epsilon) / 2f / Physics.WorldScalar
                    };
                    break;
                case RigidbodyShape.ConvexMesh:
                    fixtureDef.shape = new PolygonShape();
                    break;
            }

            ID = Physics.CreatePhysicsBody(bodyDef, fixtureDef, massData, Mode == RigidbodyMode.Kinematic);
            isValid = true;
        }

        public override void FixedUpdate()
        {
            if (!isValid || ID == uint.MaxValue || IsDestroyed) return;

            // Setting values for interpolation
            oldPosition = transform.Position;
            oldAngle = transform.Rotation;

            newPosition = Physics.GetBodyPosition(ID);
            newAngle = Physics.GetBodyRotation(ID);

            lerpDeltaTime = Time.FixedDeltaTime;
            lastLerpTime = (float)GLFW.Glfw.Time;

            interpolationReady = true;
        }

        public override void Update()
        {
            if (!isValid || ID == uint.MaxValue || !interpolationReady || IsDestroyed) return;

            // Interpolation
            float t = MathEx.Clamp01((Time.GameTime - lastLerpTime) / lerpDeltaTime);

            //      Position
            transform.Position = Vector2.Lerp(oldPosition, newPosition, t);

            //      Rotation
            transform.Rotation = (float)(oldAngle * (1.0 - t) + (newAngle * t));
        }

        public Joint CreateJoint(JointDef _jointDef)
        {
            uint id = Physics.CreateJoint(_jointDef);
            Joints.Add(id);
            return joint;
        }
    }

    public enum RigidbodyShape
    {
        Box,
        Circle,
        ConvexMesh
    }

    public enum RigidbodyMode
    {
        Kinematic,
        Dynamic,
    }

    /// <summary>
    /// Defines a static rigidbody.
    /// </summary>
    public struct RigidbodyStaticDef
    {
        /// <summary>
        /// The friction of the rigidbody against other rigidbodies.
        /// </summary>
        public float Friction = 1;
        /// <summary>
        /// The bounciness of the rigidbody. Also called Restitution.
        /// </summary>
        public float Bounciness = 0;
        /// <summary>
        /// The shape of the rigidbody.
        /// </summary>
        public RigidbodyShape Shape = RigidbodyShape.Box;
        /// <summary>
        /// The layer of this rigidbody determines how other rigidbodies will interact with this one.
        /// </summary>
        public ushort Layer = 0x0001;
        /// <summary>
        /// The hit mask determines what layers this rigidbody can collide with.
        /// </summary>
        public ushort HitMask = 0xFFFF;
        /// <summary>
        /// If either rigidbody has a GroupIndex of zero, use the category/mask rules as above.
        /// If both GroupIndex values are non-zero but different, use the category/mask rules as above.
        /// If both GroupIndex values are the same and positive, collide.
        /// If both GroupIndex values are the same and negative, don't collide.
        /// </summary>
        public short GroupIndex = 0;
        /// <summary>
        /// Vertex array for a custom convex collider. Only used when <see cref="Shape"/> is set to <see cref="RigidbodyShape.ConvexMesh"/>
        /// </summary>
        public Vector2[] ConvexColliderPoints = new Vector2[0];

        public RigidbodyStaticDef() { }
    }

    /// <summary>
    /// Defines a dynamic rigidbody.
    /// </summary>
    public struct RigidbodyDynamicDef
    {
        /// <summary>
        /// The starting linear velocity of the rigidbody.
        /// </summary>
        public Vector2 Velocity = Vector2.Zero;
        /// <summary>
        /// The starting angular velocity of the rigidbody.
        /// </summary>
        public float AngularVelocity = 0;
        /// <summary>
        /// The friction of the rigidbody against other rigidbodies.
        /// </summary>
        public float Friction = 1;
        /// <summary>
        /// The bounciness of the rigidbody. Also called Restitution.
        /// </summary>
        public float Bounciness = 0;
        /// <summary>
        /// The dampening of the rigidbody's linear velocity.
        /// </summary>
        public float LinearDampening = 0.0f;
        /// <summary>
        /// The dampening of the rigidbody's angular velocity.
        /// </summary>
        public float AngularDampening = 0.01f;
        /// <summary>
        /// The density of the rigidbody./>
        /// </summary>
        public float Density = 1;
        /// <summary>
        /// The mass data of the rigidbody.
        /// </summary>
        public MassData MassData = new MassData();
        /// <summary>
        /// Whether the rigidbody can rotate in the simulation.
        /// </summary>
        public bool FixedRotation = false;
        /// <summary>
        /// Whether the physics simulation should use continuous detection when this dynamic rigidbody hits another dynamic rigidbody.
        /// This is set to false by default because it is costly to use for all dynamic objects, so only enable this when the rigidbody will be moving very quickly and
        /// hitting other dynamic rigidbodies.
        /// </summary>
        public bool IsBullet = false;
        /// <summary>
        /// The shape of the rigidbody.
        /// </summary>
        public RigidbodyShape Shape = RigidbodyShape.Box;
        /// <summary>
        /// The layer of this rigidbody determines how other rigidbodies will interact with this one.
        /// </summary>
        public ushort Layer = 0x0001;
        /// <summary>
        /// The hit mask determines what layers this rigidbody can collide with.
        /// </summary>
        public ushort HitMask = 0xFFFF;
        /// <summary>
        /// If either rigidbody has a GroupIndex of zero, use the category/mask rules as above.
        /// If both GroupIndex values are non-zero but different, use the category/mask rules as above.
        /// If both GroupIndex values are the same and positive, collide.
        /// If both GroupIndex values are the same and negative, don't collide.
        /// </summary>
        public short GroupIndex = 0;
        /// <summary>
        /// Vertex array for a custom convex collider. Only used when <see cref="Shape"/> is set to <see cref="RigidbodyShape.ConvexMesh"/>
        /// </summary>
        public Vector2[] ConvexColliderPoints = new Vector2[0];

        public RigidbodyDynamicDef() { }
    }
}
