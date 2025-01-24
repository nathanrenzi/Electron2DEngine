using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.Joints.Wheel;
using System.Numerics;

namespace Electron2D.PhysicsBox2D
{
    public class RigidbodyWheelJointDef : IRigidbodyJointDef
    {
        public Rigidbody RigidbodyA { get; set; }
        public Rigidbody RigidbodyB { get; set; }
        public bool CollideConnected { get; set; }
        public float Damping { get; set; }
        public bool EnableLimit { get; set; }
        public bool EnableMotor { get; set; }
        public Vector2 LocalAnchorA { get; set; }
        public Vector2 LocalAnchorB { get; set; }
        public Vector2 LocalAxisA { get; set; }
        public float LowerTranslation { get; set; }
        public float MaxMotorTorque { get; set; }
        public float MotorSpeed { get; set; }
        public float Stiffness { get; set; }
        public float UpperTranslation { get; set; }

        public JointDef GetPhysicsDefinition()
        {
            WheelJointDef def = new WheelJointDef();
            def.bodyA = RigidbodyA.PhysicsBody;
            def.bodyB = RigidbodyB.PhysicsBody;
            def.collideConnected = CollideConnected;
            def.damping = Damping;
            def.enableLimit = EnableLimit;
            def.enableMotor = EnableMotor;
            def.localAnchorA = LocalAnchorA;
            def.localAnchorB = LocalAnchorB;
            def.localAxisA = LocalAxisA;
            def.lowerTranslation = LowerTranslation;
            def.maxMotorTorque = MaxMotorTorque;
            def.motorSpeed = MotorSpeed;
            def.stiffness = Stiffness;
            def.upperTranslation = UpperTranslation;
            return def;
        }
    }
}
