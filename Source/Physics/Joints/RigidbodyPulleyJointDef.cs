using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.Joints.Pulley;
using System.Numerics;

namespace Electron2D.PhysicsBox2D
{
    public class RigidbodyPulleyJointDef : IRigidbodyJointDef
    {
        public Rigidbody RigidbodyA { get; set; }
        public Rigidbody RigidbodyB { get; set; }
        public bool CollideConnected { get; set; }
        public Vector2 GroundAnchorA { get; set; }
        public Vector2 GroundAnchorB { get; set; }
        public float LengthA { get; set; }
        public float LengthB { get; set; }
        public Vector2 LocalAnchorA { get; set; }
        public Vector2 LocalAnchorB { get; set; }
        public float MaxLength1 { get; set; }
        public float MaxLength2 { get; set; }
        public float Ratio { get; set; }

        public JointDef GetPhysicsDefinition()
        {
            // def has initialization function, might want to wrap that too

            PulleyJointDef def = new PulleyJointDef();
            def.bodyA = RigidbodyA.PhysicsBody;
            def.bodyB = RigidbodyB.PhysicsBody;
            def.collideConnected = CollideConnected;
            def.GroundAnchorA = GroundAnchorA / Physics.WorldScalar;
            def.GroundAnchorB = GroundAnchorB / Physics.WorldScalar;
            def.LengthA = LengthA / Physics.WorldScalar;
            def.LengthB = LengthB / Physics.WorldScalar;
            def.LocalAnchorA = LocalAnchorA / Physics.WorldScalar;
            def.LocalAnchorB = LocalAnchorB / Physics.WorldScalar;
            def.MaxLength1 = MaxLength1 / Physics.WorldScalar;
            def.MaxLength2 = MaxLength2 / Physics.WorldScalar;
            return def;
        }
    }
}
