using Box2DX.Common;
using Box2DX.Dynamics;
using Electron2D.Core.ECS;
using System.Numerics;

namespace Electron2D.Core.PhysicsBox2D
{
    public class RigidbodySensorSystem : BaseSystem<RigidbodySensor> { }
    public class RigidbodySensor : Component
    {
        public static readonly float Epsilon = 1f;

        public Action<Rigidbody> OnBeginContact { get; set; }
        public Action<Rigidbody> OnEndContact { get; set; }
        public List<Rigidbody> CurrentContacts { get; set; } = new List<Rigidbody>();

        public uint ID { get; private set; } = uint.MaxValue;
        public ColliderSensorShape Shape { get; private set; }
        public Vector2 Size { get; private set; }
        public Vector2 Offset { get; private set; }

        // https://www.iforce2d.net/b2dtut/collision-filtering
        /// <summary>
        /// The layer of this sensor determines how rigidbodies will interact with it.
        /// </summary>
        public ushort Layer { get; private set; }
        /// <summary>
        /// The hit mask determines what layers this sensor can collide with.
        /// </summary>
        public ushort HitMask { get; private set; }
        /// <summary>
        /// If either body has a GroupIndex of zero, use the category/mask rules as above.
        /// If both GroupIndex values are non-zero but different, use the category/mask rules as above.
        /// If both GroupIndex values are the same and positive, collide.
        /// If both GroupIndex values are the same and negative, don't collide.
        /// </summary>
        public short GroupIndex { get; private set; }

        private Transform transform;
        private bool isValid;

        public RigidbodySensor(Vector2 _size, ColliderSensorShape _shape = ColliderSensorShape.Circle,
            ushort _layer = 0x0001, ushort _hitMask = 0xFFFF, short _groupIndex = 0)
        {
            Shape = _shape;
            Size = _size;
            Layer = _layer;
            HitMask = _hitMask;
            GroupIndex = _groupIndex;

            RigidbodySensorSystem.Register(this);
        }

        public RigidbodySensor(Vector2 _size, Vector2 _localOffset, ColliderSensorShape _shape = ColliderSensorShape.Circle,
            ushort _layer = 0x0001, ushort _hitMask = 0xFFFF, short _groupIndex = 0)
        {
            Shape = _shape;
            Size = _size;
            Offset = _localOffset;
            Layer = _layer;
            HitMask = _hitMask;
            GroupIndex = _groupIndex;

            RigidbodySensorSystem.Register(this);
        }

        ~RigidbodySensor()
        {
            RigidbodySensorSystem.Unregister(this);
        }

        public static void InvokeSensor(uint _id, uint _hitId, bool _beginContact)
        {
            List<Rigidbody> rigidbodies = RigidbodySystem.GetComponents();
            List<RigidbodySensor> sensors = RigidbodySensorSystem.GetComponents();

            Rigidbody hitBody = null;
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                if (rigidbodies[i].ID == _hitId)
                {
                    hitBody = rigidbodies[i];
                }
            }
            if (hitBody == null) return;

            RigidbodySensor sensor = null;
            for (int i = 0; i < sensors.Count; i++)
            {
                if (sensors[i].ID == _id)
                {
                    sensor = sensors[i];
                }
            }
            if (sensor == null) return;

            if(_beginContact)
            {
                // Begin Contact
                sensor.OnBeginContact?.Invoke(hitBody);
                sensor.CurrentContacts.Add(hitBody);
            }
            else
            {
                // End Contact
                sensor.OnEndContact?.Invoke(hitBody);
                sensor.CurrentContacts.Remove(hitBody);
            }
        }

        public override void OnAdded()
        {
            transform = GetComponent<Transform>();
            if (transform == null)
            {
                Debug.LogError("PHYSICS: A collider sensor is trying to be added to an entity without a Transform component, removing collider...");
                Entity.RemoveComponent(this);
                isValid = false;
                Dispose();
                return;
            }

            Vector2 pos = (transform.Position + (transform.Up * Offset.Y) + (transform.Right * Offset.X)) / Physics.WorldScalar;
            BodyDef bodyDef = new BodyDef()
            {
                Position = new Vec2(pos.X, pos.Y),
                Angle = transform.Rotation,
            };

            FixtureDef fixtureDef;
            if (Shape == ColliderSensorShape.Box)
            {
                PolygonDef polygonDef = new PolygonDef()
                {
                    Filter = new FilterData()
                    {
                        CategoryBits = Layer,
                        MaskBits = HitMask,
                        GroupIndex = GroupIndex
                    },
                    IsSensor = true
                };
                polygonDef.SetAsBox((Size.X - Epsilon) / 2f / Physics.WorldScalar, (Size.Y - Epsilon) / 2f / Physics.WorldScalar);
                fixtureDef = polygonDef;
            }
            else
            {
                CircleDef circleDef = new CircleDef()
                {
                    Radius = (Size.X - Epsilon) / 2f / Physics.WorldScalar,
                    Filter = new FilterData()
                    {
                        CategoryBits = Layer,
                        MaskBits = HitMask,
                        GroupIndex = GroupIndex
                    },
                    IsSensor = true
                };
                fixtureDef = circleDef;
            }

            ID = Physics.CreatePhysicsBody(bodyDef, fixtureDef);
            isValid = true;
        }

        public override void Update()
        {
            if (!isValid || ID == uint.MaxValue) return;

            Physics.SetAngle(ID, transform.Rotation);
            Physics.SetPosition(ID, (transform.Position + (transform.Up * Offset.Y) + (transform.Right * Offset.X)) / Physics.WorldScalar);
        }
    }

    public enum ColliderSensorShape
    {
        Box,
        Circle
    }
}