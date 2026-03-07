namespace Electron2D.UI
{
    public struct Border
    {
        public float Left, Top, Right, Bottom;

        public Border(float uniform)
        {
            Left = Top = Right = Bottom = uniform;
        }

        public Border(float horizontal, float vertical)
        {
            Left = Right = horizontal;
            Top = Bottom = vertical;
        }

        public Border(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public override string ToString()
        {
            return $"(L: {Left}, T: {Top}, R: {Right}, B: {Bottom})";
        }
    }
}
