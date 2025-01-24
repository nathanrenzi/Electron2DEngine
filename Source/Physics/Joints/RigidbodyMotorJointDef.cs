using Box2D.NetStandard.Dynamics.Joints;

namespace Electron2D.PhysicsBox2D
{
    public class RigidbodyMotorJointDef : IRigidbodyJointDef
    {
        public Rigidbody RigidbodyA { get; set; }
        public Rigidbody RigidbodyB { get; set; }
        public bool CollideConnected { get; set; }

        public JointDef GetPhysicsDefinition()
        {
            throw new NotImplementedException();
        }
    }
}
