namespace Electron2D.UserInterface
{
    public class SlicedPanelDef
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
        public float BorderPixelSize;

        /// <param name="left">The left padding percentage of the 9-sliced texture.</param>
        /// <param name="right">The right padding percentage of the 9-sliced texture.</param>
        /// <param name="top">The top padding percentage of the 9-sliced texture.</param>
        /// <param name="bottom">The bottom padding percentage of the 9-sliced texture.</param>
        /// <param name="borderPixelSize">The size of the border.</param>
        public SlicedPanelDef(float left, float right, float top, float bottom, float borderPixelSize)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
            BorderPixelSize = borderPixelSize;
        }
    }
}
