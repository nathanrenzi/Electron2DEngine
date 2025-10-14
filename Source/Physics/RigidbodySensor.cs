using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using System.Numerics;

namespace Electron2D.PhysicsBox2D
{
    public class RigidbodySensor : IGameClass
    {
        public static readonly float Epsilon = 1f;
        public static List<RigidbodySensor> Sensors = new List<RigidbodySensor>();

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

        private Transform _transform;
        private bool _isValid;

        public RigidbodySensor(Vector2 size, RigidbodySensorShape shape = RigidbodySensorShape.Circle,
            ushort layer = 0x0001, ushort hitMask = 0xFFFF, short groupIndex = 0)
        {
            Shape = shape;
            Size = size;
            Layer = layer;
            HitMask = hitMask;
            GroupIndex = groupIndex;
            Initialize();
        }

        public RigidbodySensor(Transform transform, Vector2 size, Vector2 localOffset, RigidbodySensorShape shape = RigidbodySensorShape.Circle,
            ushort layer = 0x0001, ushort hitMask = 0xFFFF, short groupIndex = 0)
        {
            _transform = transform;
            Shape = shape;
            Size = size;
            Offset = localOffset;
            Layer = layer;
            HitMask = hitMask;
            GroupIndex = groupIndex;
            Initialize();
        }

        private void Initialize()
        {
            if (_transform == null)
            {
                Debug.LogError("PHYSICS: A collider sensor is trying to be added to an entity without a Transform component, removing collider...");
                _isValid = false;
                Dispose();
                return;
            }
            Engine.Game.RegisterGameClass(this);
            Sensors.Add(this);

            Vector2 pos = (_transform.Position + (_transform.Up * Offset.Y) + (_transform.Right * Offset.X));
            BodyDef bodyDef = new BodyDef()
            {
                position = pos,
                angle = _transform.Rotation,
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
            switch (Shape)
            {
                case RigidbodySensorShape.Box:
                    fixtureDef.shape = new PolygonShape((_transform.Scale.X - Epsilon) / 2f / Physics.WorldScalar,
                        (_transform.Scale.Y - Epsilon) / 2f / Physics.WorldScalar);
                    break;
                case RigidbodySensorShape.Circle:
                    fixtureDef.shape = new CircleShape()
                    {
                        Radius = (Size.X - Epsilon) / 2f / Physics.WorldScalar,
                    };
                    break;
            }

            ID = Physics.CreatePhysicsBody(bodyDef, fixtureDef, new MassData(), true);
            _isValid = true;
        }

        ~RigidbodySensor()
        {
            Dispose();
        }

        public void FixedUpdate() { }

        public void Dispose()
        {
            Engine.Game.UnregisterGameClass(this);
            Sensors.Remove(this);
            GC.SuppressFinalize(this);
        }

        public static void InvokeCollision(uint _id, uint _hitId, bool _beginContact)
        {
            Rigidbody hitBody = null;
            List<Rigidbody> rigidbodies = Rigidbody.Rigidbodies;
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                if (rigidbodies[i].ID == _hitId)
                {
                    hitBody = rigidbodies[i];
                }
            }
            if (hitBody == null) return;

            RigidbodySensor sensor = null;
            for (int i = 0; i < Sensors.Count; i++)
            {
                if (Sensors[i].ID == _id)
                {
                    sensor = Sensors[i];
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

        public void Update()
        {
            if (!_isValid || ID == uint.MaxValue) return;

            Physics.SetAngle(ID, _transform.Rotation);
            Physics.SetPosition(ID, _transform.Position + (_transform.Up * Offset.Y) + (_transform.Right * Offset.X));
        }
    }

    public enum RigidbodySensorShape
    {
        Box,
        Circle
    }
}