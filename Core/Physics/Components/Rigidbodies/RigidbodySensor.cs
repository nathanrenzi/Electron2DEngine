using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
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
        public RigidbodySensorShape Shape { get; private set; }
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

        public RigidbodySensor(Vector2 _size, RigidbodySensorShape _shape = RigidbodySensorShape.Circle,
            ushort _layer = 0x0001, ushort _hitMask = 0xFFFF, short _groupIndex = 0)
        {
            Shape = _shape;
            Size = _size;
            Layer = _layer;
            HitMask = _hitMask;
            GroupIndex = _groupIndex;

            RigidbodySensorSystem.Register(this);
        }

        public RigidbodySensor(Vector2 _size, Vector2 _localOffset, RigidbodySensorShape _shape = RigidbodySensorShape.Circle,
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

        public static void InvokeCollision(uint _id, uint _hitId, bool _beginContact)
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

            Vector2 pos = (transform.Position + (transform.Up * Offset.Y) + (transform.Right * Offset.X));
            BodyDef bodyDef = new BodyDef()
            {
                position = pos,
                angle = transform.Rotation,
            };

            FixtureDef fixtureDef = new FixtureDef()
            {
                filter = new Filter()
                {
                    categoryBits = Layer,
                    maskBits = HitMask,
                    groupIndex = GroupIndex
                },
                isSensor = true,
            };
            switch(this.Shape)
            {
                case RigidbodySensorShape.Box:
                    fixtureDef.shape = new PolygonShape((transform.Scale.X - Epsilon) / 2f / Physics.WorldScalar, (transform.Scale.Y - Epsilon) / 2f / Physics.WorldScalar);
                    break;
                case RigidbodySensorShape.Circle:
                    fixtureDef.shape = new CircleShape()
                    {
                        Radius = (Size.X - Epsilon) / 2f / Physics.WorldScalar,
                    };
                    break;
            }

            ID = Physics.CreatePhysicsBody(bodyDef, fixtureDef, new MassData(), true);
            isValid = true;
        }

        public override void Update()
        {
            if (!isValid || ID == uint.MaxValue) return;

            Physics.SetAngle(ID, transform.Rotation);
            Physics.SetPosition(ID, transform.Position + (transform.Up * Offset.Y) + (transform.Right * Offset.X));
        }
    }

    public enum RigidbodySensorShape
    {
        Box,
        Circle
    }
}