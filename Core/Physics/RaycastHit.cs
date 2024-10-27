using Box2D.NetStandard.Dynamics.Fixtures;
using System.Numerics;

namespace Electron2D.Core.PhysicsBox2D
{
    public class RaycastHit
    {
        public bool Hit;
        public Vector2 Normal;
        public Vector2 Point;
        public float Fraction;
        public Fixture Fixture;
        public float Distance = 0;
        public float MaxDistance = 0;

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
            Hit = fixture != null;
            Fixture = fixture;
            Point = point * Physics.WorldScalar;
            Normal = normal;
            Fraction = fraction;
            Distance = fraction * MaxDistance;
        }

        public string ToString()
        {
            return $"Hit: {Hit}, Normal: {Normal}, Point: {Point}, Fraction: {Fraction}, Distance: {Distance}, MaxDistance: {MaxDistance}";
        }
    }
}
