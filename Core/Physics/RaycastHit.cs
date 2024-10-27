using Box2D.NetStandard.Dynamics.Fixtures;
using System.Numerics;

namespace Electron2D.Core.PhysicsBox2D
{
    public struct RaycastHit
    {
        public bool Hit;
        public Vector2 Normal;
        public Vector2 Point;
        public float Fraction;
        public Fixture Fixture;

        public RaycastHit()
        {
            Hit = false;
            Normal = Vector2.Zero;
            Point = Vector2.Zero;
            Fraction = 0;
            Fixture = null;
        }

        public void Callback(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
            Hit = true;
            Fixture = fixture;
            Point = point;
            Normal = normal;
            Fraction = fraction;
        }
    }
}
