using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.Joints.Distance;
using System.Numerics;

namespace Electron2D.Core.PhysicsBox2D
{
    public class RigidbodyDistanceJointDef : IRigidbodyJointDef
    {
        public Rigidbody RigidbodyA { get; set; }
        public Rigidbody RigidbodyB { get; set; }
        public Vector2 LocalAnchorA { get; set; }
        public Vector2 LocalAnchorB { get; set; }
        public float Length { get; set; }
        public float Damping { get; set; }
        public float Stiffness { get; set; }
        public bool CollideConnected { get; set; }

        public JointDef GetPhysicsDefinition()
        {
            DistanceJointDef d = new DistanceJointDef();
            d.bodyA = RigidbodyA.PhysicsBody;
            d.bodyB = RigidbodyB.PhysicsBody;
            d.length = Length / Physics.WorldScalar;
            d.localAnchorA = LocalAnchorA / Physics.WorldScalar;
            d.localAnchorB = LocalAnchorB / Physics.WorldScalar;
            d.damping = Damping;
            d.stiffness = Stiffness;
            d.collideConnected = CollideConnected;
            return d;
        }
    }
}
