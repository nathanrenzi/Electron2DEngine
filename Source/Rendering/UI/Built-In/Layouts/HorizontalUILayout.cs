using System.Numerics;

namespace Atlas2D.UI
{
    /// <summary>
    /// A <see cref="LinearUILayout"/> that arranges children horizontally.
    /// </summary>
    public sealed class HorizontalUILayout : LinearUILayout
    {
        protected override float GetMain(Vector2 v) => v.X;
        protected override float GetCross(Vector2 v) => v.Y;
        protected override Vector2 MakeVector(float main, float cross) => new Vector2(main, cross);
    }
}
