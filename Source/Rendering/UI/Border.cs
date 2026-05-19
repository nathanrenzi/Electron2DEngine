namespace Atlas2D.UI
{
    /// <summary>
    /// Defines a four-sided border with independent left, top, right, and bottom values.
    /// Used for padding, margin, and UV border regions.
    /// </summary>
    public struct Border
    {
        /// <summary>The left border value.</summary>
        public float Left;

        /// <summary>The top border value.</summary>
        public float Top;

        /// <summary>The right border value.</summary>
        public float Right;

        /// <summary>The bottom border value.</summary>
        public float Bottom;

        /// <summary>
        /// Creates a new <see cref="Border"/> with a uniform value on all sides.
        /// </summary>
        /// <param name="uniform">The value applied to all four sides.</param>
        public Border(float uniform)
        {
            Left = Top = Right = Bottom = uniform;
        }

        /// <summary>
        /// Creates a new <see cref="Border"/> with independent horizontal and vertical values.
        /// </summary>
        /// <param name="horizontal">The value applied to the left and right sides.</param>
        /// <param name="vertical">The value applied to the top and bottom sides.</param>
        public Border(float horizontal, float vertical)
        {
            Left = Right = horizontal;
            Top = Bottom = vertical;
        }

        /// <summary>
        /// Creates a new <see cref="Border"/> with independent values for each side.
        /// </summary>
        /// <param name="left">The left border value.</param>
        /// <param name="top">The top border value.</param>
        /// <param name="right">The right border value.</param>
        /// <param name="bottom">The bottom border value.</param>
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
