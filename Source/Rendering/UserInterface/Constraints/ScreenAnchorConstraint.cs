using System.ComponentModel;
using System.Numerics;

namespace Electron2D.UserInterface
{
    public enum ScreenAnchorConstraintMode
    {
        BothAxes,
        XOnly,
        YOnly
    }

    public class ScreenAnchorConstraint : UIConstraint
    {
        public Vector2 Offset { get; set; }
        private readonly Vector2 _anchorPosition;
        private readonly ScreenAnchorConstraintMode _mode;
        private static float LeftWindowBound => 0;
        private static float RightWindowBound => Display.WindowSize.X;
        private static float TopWindowBound => 0;
        private static float BottomWindowBound => Display.WindowSize.Y;

        /// <summary>
        /// Anchors a component to a virtual position on the screen.
        /// (-1, 1) = top-left, (1, -1) = bottom-right, (0, 0) = center.
        /// </summary>
        public ScreenAnchorConstraint(Vector2 anchorPosition, Vector2 offset = default, ScreenAnchorConstraintMode mode = ScreenAnchorConstraintMode.BothAxes)
        {
            _anchorPosition = Vector2.Clamp(anchorPosition, new Vector2(-1), new Vector2(1));
            Offset = offset;
            _mode = mode;
        }

        private Vector2 CalculatePosition(Vector2 position)
        {
            float targetX = MathEx.Lerp(LeftWindowBound, RightWindowBound, (_anchorPosition.X + 1f) * 0.5f);
            float targetY = MathEx.Lerp(BottomWindowBound, TopWindowBound, (_anchorPosition.Y + 1f) * 0.5f);

            if (_mode == ScreenAnchorConstraintMode.XOnly || _mode == ScreenAnchorConstraintMode.BothAxes)
            {
                position.X = targetX + Offset.X;
            }
            if (_mode == ScreenAnchorConstraintMode.YOnly || _mode == ScreenAnchorConstraintMode.BothAxes)
            {
                position.Y = targetY + Offset.Y;
            }

            return position;
        }

        public override void ApplyConstraint(UIComponent component)
        {
            component.Transform.Position = CalculatePosition(component.Transform.Position);
        }

        public override bool CheckConstraint(UIComponent component)
        {
            Vector2 position = CalculatePosition(component.Transform.Position);
            if(_mode == ScreenAnchorConstraintMode.XOnly || _mode == ScreenAnchorConstraintMode.BothAxes)
            {
                if(component.Transform.Position.X != position.X)
                {
                    return false;
                }
            }
            if(_mode == ScreenAnchorConstraintMode.YOnly || _mode == ScreenAnchorConstraintMode.BothAxes)
            {
                if(component.Transform.Position.Y != position.Y)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
