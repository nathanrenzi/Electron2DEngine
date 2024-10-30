using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.Joints.Mouse;
using System.Numerics;

namespace Electron2D.Core.PhysicsBox2D
{
    public class RigidbodyMouseJointDef : IRigidbodyJointDef
    {
        public Rigidbody RigidbodyA { get; set; }
        public Rigidbody RigidbodyB { get; set; }
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
