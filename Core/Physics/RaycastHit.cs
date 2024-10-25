using Box2DX.Dynamics;
using System.Numerics;

namespace Electron2D.Core.PhysicsBox2D
{
    public struct RaycastHit
    {
        public bool Hit;
        public Vector2 Normal;
        public Vector2 Point;
        public float Distance;
        public Fixture Fixture;
    }
}
