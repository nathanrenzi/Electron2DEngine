using Box2D.NetStandard.Dynamics.Joints;

namespace Atlas2D.PhysicsBox2D
{
    public class RigidbodyMotorJointDef : IRigidbodyJointDef
    {
        public RigidbodyNode RigidbodyA { get; set; }
        public RigidbodyNode RigidbodyB { get; set; }
        public bool CollideConnected { get; set; }

        public JointDef GetPhysicsDefinition()
        {
            throw new NotImplementedException();
        }
    }
}
