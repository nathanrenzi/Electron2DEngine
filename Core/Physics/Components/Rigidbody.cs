using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
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
        public RigidbodyMassMode MassMode { get; private set; }
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
                _definition.MassMode, _definition.Layer, _definition.HitMask, _definition.GroupIndex);
        }

        public static Rigidbody CreateStatic(RigidbodyStaticDef _definition)
        {
            return new Rigidbody(true, new Vector2(0, 0), 0, new MassData(), _definition.Friction, _definition.Bounciness, 1, 0.0f, 0.0f, false, _definition.Shape,
                RigidbodyMassMode.ManualMassUseData, _definition.Layer, _definition.HitMask, _definition.GroupIndex);
        }

        private Rigidbody(bool _isStatic, Vector2 _startVelocity, float _startAngularVelocity, MassData _massData, float _friction, float _bounciness,
            float _density, float _linearDampening, float _angularDampening, bool _fixedRotation, RigidbodyShape _rigidbodyShape,
            RigidbodyMassMode _rigidbodyMode, ushort _layer, ushort _hitMask, short _groupIndex)
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
            MassMode = _rigidbodyMode;
            Shape = _rigidbodyShape;
            Layer = _layer;
            HitMask = _hitMask;
            GroupIndex = _groupIndex;

            RigidbodySystem.Register(this);
        }

        protected override void OnDispose()
        {
            RigidbodySystem.Unregister(this);
            if(ID != uint.MaxValue) Physics.RemovePhysicsBody(ID);
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
                Debug.LogError("PHYSICS: A rigidbody is trying to be added to an entity without a Transform component, removing collider...");
                Entity.RemoveComponent(this);
                isValid = false;
                Dispose();
                return;
            }

            BodyDef bodyDef = new BodyDef()
            {
                Position = new Vec2(transform.Position.X / Physics.WorldScalar, transform.Position.Y / Physics.WorldScalar),
                Angle = transform.Rotation,
                LinearVelocity = new Vec2(velocity.X, velocity.Y),
                AngularVelocity = angularVelocity,
                LinearDamping = linearDampening,
                AngularDamping = angularDampening,
                FixedRotation = fixedRotation
            };

            FixtureDef fixtureDef;
            if(Shape == RigidbodyShape.Box)
            {
                PolygonDef polygonDef = new PolygonDef()
                {
                    Density = density,
                    Friction = friction,
                    Restitution = bounciness,
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
                    Restitution = bounciness,
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
            
            if(!IsStatic)
            {
                if(MassMode == RigidbodyMassMode.ManualMassUseData)
                {
                    // Normal Mass
                    ID = Physics.CreatePhysicsBody(bodyDef, fixtureDef, massData);
                    isValid = true;
                }
                else if(MassMode == RigidbodyMassMode.AutoMassIgnoreData)
                {
                    // Auto Mass
                    ID = Physics.CreatePhysicsBody(bodyDef, fixtureDef, true);
                    isValid = true;
                }
            }
            else
            {
                // Static Rigidbody
                ID = Physics.CreatePhysicsBody(bodyDef, fixtureDef, false);
                isValid = true;
            }
        }

        public override void FixedUpdate()
        {
            if (!isValid || ID == uint.MaxValue) return;

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
            if (!isValid || ID == uint.MaxValue || !interpolationReady) return;

            // Interpolation
            float t = MathEx.Clamp01((Time.GameTime - lastLerpTime) / lerpDeltaTime);

            //      Position
            transform.Position = Vector2.Lerp(oldPosition, newPosition, t);

            //      Rotation
            transform.Rotation = (float)(oldAngle * (1.0 - t) + (newAngle * t));
        }
    }

    public enum RigidbodyShape
    {
        Box,
        Circle,
        ConvexMesh
    }

    public enum RigidbodyMassMode
    {
        AutoMassIgnoreData,
        ManualMassUseData,
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
        /// The shape of the rigidbody. NOTE: CONVEX MESH MODE IS NOT IMPLEMENTED YET
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
        /// The density of the rigidbody. This is used when the mass mode is set to <see cref="RigidbodyMassMode.AutoMassIgnoreData"/>
        /// </summary>
        public float Density = 1;
        /// <summary>
        /// The mass data of the rigidbody. This will only be used when <see cref="MassMode"/> is set to <see cref="RigidbodyMassMode.ManualMassUseData"/>
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
        /// The shape of the rigidbody. NOTE: CONVEX MESH MODE IS NOT IMPLEMENTED YET
        /// </summary>
        public RigidbodyShape Shape = RigidbodyShape.Box;
        /// <summary>
        /// The mass mode of the rigidbody. The <see cref="MassData"/> field will only be used when this is set to <see cref="RigidbodyMassMode.ManualMassUseData"/>
        /// </summary>
        public RigidbodyMassMode MassMode = RigidbodyMassMode.AutoMassIgnoreData;
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

        public RigidbodyDynamicDef() { }
    }
}
