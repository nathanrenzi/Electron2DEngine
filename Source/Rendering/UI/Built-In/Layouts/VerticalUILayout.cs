using System.Numerics;

namespace Electron2D.UI
{
    public sealed class VerticalUILayout : LinearUILayout
    {
        protected override float GetMain(Vector2 v) => v.Y;
        protected override float GetCross(Vector2 v) => v.X;
        protected override Vector2 MakeVector(float main, float cross) => new Vector2(cross, main);
    }
}
