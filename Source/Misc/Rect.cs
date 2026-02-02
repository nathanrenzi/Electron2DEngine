namespace Electron2D
{
    public struct Rect
    {
        public float X, Y, Width, Height;

        public Rect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public static bool operator ==(Rect a, Rect b)
        {
            return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
        }

        public static bool operator !=(Rect a, Rect b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return $"(X: {X}, Y: {Y}, Width: {Width}, Height: {Height})";
        }
    }
}
