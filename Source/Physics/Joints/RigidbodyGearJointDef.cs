using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.Joints.Gear;

namespace Electron2D.PhysicsBox2D
{
    public class RigidbodyGearJointDef : IRigidbodyJointDef
    {
        public Rigidbody RigidbodyA { get; set; }
        public Rigidbody RigidbodyB { get; set; }
        public Joint Joint1 { get; set; }
        public Joint Joint2 { get; set; }
        public float Ratio { get; set; }
        public bool CollideConnected { get; set; }

        public JointDef GetPhysicsDefinition()
        {
            GearJointDef def = new GearJointDef();
            def.bodyA = RigidbodyA.PhysicsBody;
            def.bodyB = RigidbodyB.PhysicsBody;
            def.Ratio = Ratio;
            def.Joint1 = Joint1;
            def.Joint2 = Joint2;
            def.collideConnected = CollideConnected;
            return def;
        }
    }
}