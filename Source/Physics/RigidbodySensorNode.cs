using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using System.Numerics;

namespace Atlas2D.PhysicsBox2D
{
    public class RigidbodySensorNode : TransformNode
    {
        public static readonly float Epsilon = 1f;
        public static List<RigidbodySensorNode> Sensors = new List<RigidbodySensorNode>();

        public Action<RigidbodyNode> OnBeginContact { get; set; }
        public Action<RigidbodyNode> OnEndContact { get; set; }
        public List<RigidbodyNode> CurrentContacts { get; set; } = new List<RigidbodyNode>();

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

        private bool _isValid;

        public RigidbodySensorNode(Vector2 size, Vector2 localOffset = default, RigidbodySensorShape shape = RigidbodySensorShape.Circle,
            ushort layer = 0x0001, ushort hitMask = 0xFFFF, short groupIndex = 0)
        {
            Shape = shape;
            Size = size;
            Offset = localOffset;
            Layer = layer;
            HitMask = hitMask;
            GroupIndex = groupIndex;

            Sensors.Add(this);

            Vector2 pos = WorldPosition + (Up * Offset.Y) + (Right * Offset.X);
            BodyDef bodyDef = new BodyDef()
            {
                position = pos,
                angle = WorldRotation,
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
                    fixtureDef.shape = new PolygonShape((WorldScale.X - Epsilon) / 2f / Physics.WorldScalar,
                        (WorldScale.Y - Epsilon) / 2f / Physics.WorldScalar);
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

        protected override void OnDispose()
        {
            Sensors.Remove(this);
            if (ID != uint.MaxValue) Physics.RemovePhysicsBody(ID);
        }

        public static void InvokeCollision(uint _id, uint _hitId, bool _beginContact)
        {
            RigidbodyNode hitBody = null;
            List<RigidbodyNode> rigidbodies = RigidbodyNode.Rigidbodies;
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                if (rigidbodies[i].ID == _hitId)
                {
                    hitBody = rigidbodies[i];
                }
            }
            if (hitBody == null) return;

            RigidbodySensorNode sensor = null;
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

        protected override void OnUpdate()
        {
            if (!_isValid || ID == uint.MaxValue) return;

            Physics.SetAngle(ID, WorldRotation);
            Physics.SetPosition(ID, WorldPosition + (Up * Offset.Y) + (Right * Offset.X));
        }
    }

    public enum RigidbodySensorShape
    {
        Box,
        Circle
    }
}
