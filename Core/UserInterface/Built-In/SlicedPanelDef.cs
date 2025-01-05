namespace Electron2D.Core.UserInterface
{
    public class SlicedPanelDef
    {
        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
        public float BorderPixelSize;

        /// <param name="_left">The left padding percentage of the 9-sliced texture.</param>
        /// <param name="_right">The right padding percentage of the 9-sliced texture.</param>
        /// <param name="_top">The top padding percentage of the 9-sliced texture.</param>
        /// <param name="_bottom">The bottom padding percentage of the 9-sliced texture.</param>
        /// <param name="_borderPixelSize">The size of the border.</param>
        public SlicedPanelDef(float _left, float _right, float _top, float _bottom, float _borderPixelSize)
        {
            Left = _left;
            Right = _right;
            Top = _top;
            Bottom = _bottom;
            BorderPixelSize = _borderPixelSize;
        }
    }
}
