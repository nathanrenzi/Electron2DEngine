using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.Joints.Mouse;
using System.Numerics;

namespace Atlas2D.PhysicsBox2D
{
    public class RigidbodyMouseJointDef : IRigidbodyJointDef
    {
        public RigidbodyNode RigidbodyA { get; set; }
        public RigidbodyNode RigidbodyB { get; set; }
        public bool CollideConnected { get; set; }
        public float DampingRatio { get; set; }
        public float FrequencyHz { get; set; }
        public float MaxForce { get; set; }
        public Vector2 Target { get; set; }

        public JointDef GetPhysicsDefinition()
        {
            MouseJointDef def = new MouseJointDef();
            def.bodyA = RigidbodyA.PhysicsBody;
            def.bodyB = RigidbodyB.PhysicsBody;
            def.collideConnected = CollideConnected;
            def.DampingRatio = DampingRatio;
            def.FrequencyHz = FrequencyHz;
            def.MaxForce = MaxForce;
            def.Target = Target / Physics.WorldScalar;
            return def;
        }
    }
}
