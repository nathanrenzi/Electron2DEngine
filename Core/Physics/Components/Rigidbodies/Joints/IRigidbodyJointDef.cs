using Box2D.NetStandard.Dynamics.Joints;

namespace Electron2D.Core.PhysicsBox2D
{
    public interface IRigidbodyJointDef
    {
        public Rigidbody RigidbodyA { get; set; }
        public Rigidbody RigidbodyB { get; set; }
        public bool CollideConnected { get; set; }
        public JointDef GetPhysicsDefinition();
    }
}
