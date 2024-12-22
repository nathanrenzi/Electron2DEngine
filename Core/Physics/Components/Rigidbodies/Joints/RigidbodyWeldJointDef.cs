using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.Joints.Weld;
using System.Numerics;

namespace Electron2D.Core.PhysicsBox2D
{
    public class RigidbodyWeldJointDef : IRigidbodyJointDef
    {
        public Rigidbody RigidbodyA { get; set; }
        public Rigidbody RigidbodyB { get; set; }
        public bool CollideConnected { get; set; }
        public float Damping { get; set; }
        public float Stiffness { get; set; }
        public Vector2 LocalAnchorA { get; set; }
        public Vector2 LocalAnchorB { get; set; }
        public float ReferenceAngle { get; set; }
        [Obsolete("Use Joint.AngularStiffness to get stiffness & damping values", true)]
        public float DampingRatio { get; set; }
        [Obsolete("Use Joint.AngularStiffness to get stiffness & damping values", true)]
        public float FrequencyHz { get; set; }

        public JointDef GetPhysicsDefinition()
        {
            WeldJointDef def = new WeldJointDef();
            def.bodyA = RigidbodyA.PhysicsBody;
            def.bodyB = RigidbodyB.PhysicsBody;
            def.collideConnected = CollideConnected;
            //Joint.AngularStiffness(out Stiffness, out Damping, FrequencyHz, DampingRatio, RigidbodyA.PhysicsBody, RigidbodyB.PhysicsBody);
            def.damping = Damping;
            def.stiffness = Stiffness;
            def.localAnchorA = LocalAnchorA / Physics.WorldScalar;
            def.localAnchorB = LocalAnchorB / Physics.WorldScalar;
            def.referenceAngle = ReferenceAngle * MathF.PI / 180f;
            return def;
        }
    }
}
