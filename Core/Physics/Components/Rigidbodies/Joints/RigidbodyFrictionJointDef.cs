using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.Joints.Friction;
using Electron2D.Core.PhysicsBox2D;
using System.Numerics;

namespace Electron2D.Core.PhysicsBox2D
{
    public class RigidbodyFrictionJointDef : IRigidbodyJointDef
    {
        public Rigidbody RigidbodyA { get; set; }
        public Rigidbody RigidbodyB { get; set; }
        public Vector2 LocalAnchorA { get; set; }
        public Vector2 LocalAnchorB { get; set; }
        public float MaxForce { get; set; }
        public float MaxTorque { get; set; }
        public bool CollideConnected { get; set; }

        public JointDef GetPhysicsDefinition()
        {
            FrictionJointDef def = new FrictionJointDef();
            def.bodyA = RigidbodyA.PhysicsBody;
            def.bodyB = RigidbodyB.PhysicsBody;
            def.maxForce = MaxForce;
            def.maxTorque = MaxTorque;
            def.localAnchorA = LocalAnchorA / Physics.WorldScalar;
            def.localAnchorB = LocalAnchorB / Physics.WorldScalar;
            def.collideConnected = CollideConnected;
            return def;
        }
    }
}
