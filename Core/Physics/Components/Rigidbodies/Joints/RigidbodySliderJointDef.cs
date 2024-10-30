using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.Joints.Prismatic;
using System.Numerics;

namespace Electron2D.Core.PhysicsBox2D
{
    public class RigidbodySliderJointDef : IRigidbodyJointDef
    {
        public Rigidbody RigidbodyA { get; set; }
        public Rigidbody RigidbodyB { get; set; }
        public bool CollideConnected { get; set; }
        public bool EnableLimit { get; set; }
        public bool EnableMotor { get; set; }
        public Vector2 LocalAnchorA { get; set; }
        public Vector2 LocalAnchorB { get; set; } 
        public Vector2 LocalAxisA { get; set; }
        public float LowerTranslation { get; set; }
        public float MaxMotorForce { get; set; }
        public float MotorSpeed { get; set; }
        public float UpperTranslation { get; set; }

        public JointDef GetPhysicsDefinition()
        {
            PrismaticJointDef def = new PrismaticJointDef();
            def.bodyA = RigidbodyA.PhysicsBody;
            def.bodyB = RigidbodyB.PhysicsBody;
            def.collideConnected = CollideConnected;
            def.enableLimit = EnableLimit;
            def.enableMotor = EnableMotor;
            def.localAnchorA = LocalAnchorA / Physics.WorldScalar;
            def.localAnchorB = LocalAnchorB / Physics.WorldScalar;
            def.localAxisA = LocalAxisA;
            def.lowerTranslation = LowerTranslation;
            def.maxMotorForce = MaxMotorForce;
            def.motorSpeed = MotorSpeed;
            def.referenceAngle = def.bodyB.GetAngle() - def.bodyA.GetAngle();
            def.upperTranslation = UpperTranslation;
            return def;
        }
    }
}