using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.Joints.Revolute;
using System.Numerics;

namespace Electron2D.Core.PhysicsBox2D
{
    public class RigidbodyRevoluteJointDef : IRigidbodyJointDef
    {
        public Rigidbody RigidbodyA { get; set; }
        public Rigidbody RigidbodyB { get; set; }
        public bool CollideConnected { get; set; }
        public bool EnableLimit { get; set; }
        public bool EnableMotor { get; set; }
        public Vector2 LocalAnchorA { get; set; }
        public Vector2 LocalAnchorB { get; set; }
        public float LowerAngle { get; set; }
        public float MaxMotorTorque { get; set; }
        public float MotorSpeed { get; set; }
        public float UpperAngle { get; set; }

        public JointDef GetPhysicsDefinition()
        {
            RevoluteJointDef def = new RevoluteJointDef();
            def.bodyA = RigidbodyA.PhysicsBody;
            def.bodyB = RigidbodyB.PhysicsBody;
            def.collideConnected = CollideConnected;
            def.enableLimit = EnableLimit;
            def.enableMotor = EnableMotor;
            def.localAnchorA = LocalAnchorA / Physics.WorldScalar;
            def.localAnchorB = LocalAnchorB / Physics.WorldScalar;
            def.lowerAngle = LowerAngle * MathF.PI / 180f;
            def.upperAngle = UpperAngle * MathF.PI / 180f;
            def.maxMotorTorque = MaxMotorTorque;
            def.motorSpeed = MotorSpeed;
            def.referenceAngle = RigidbodyB.PhysicsBody.GetAngle() - RigidbodyA.PhysicsBody.GetAngle();
            return def;
        }
    }
}
