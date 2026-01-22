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
        public Vector2 Offset { get; private set; }
        public Vector2 Anchor { get; private set; }
        public ScreenAnchorConstraintMode Mode { get; private set; }
        private static float LeftWindowBound => 0;
        private static float RightWindowBound => UICanvas.Instance.ScreenToVirtual(Display.WindowSize).X;
        private static float TopWindowBound => 0;
        private static float BottomWindowBound => UICanvas.Instance.ScreenToVirtual(Display.WindowSize).Y;

        /// <summary>
        /// Anchors a component to a virtual position on the screen.
        /// (-1, -1) = top-left, (1, 1) = bottom-right, (0, 0) = center.
        /// </summary>
        public ScreenAnchorConstraint(Vector2 anchorPosition, Vector2 offset = default, ScreenAnchorConstraintMode mode = ScreenAnchorConstraintMode.BothAxes)
        {
            Anchor = Vector2.Clamp(anchorPosition, new Vector2(-1), new Vector2(1));
            Offset = offset;
            Mode = mode;
        }

        private Vector2 CalculatePosition(Vector2 position)
        {
            float targetX = MathEx.Lerp(LeftWindowBound, RightWindowBound, (Anchor.X + 1f) * 0.5f);
            float targetY = MathEx.Lerp(TopWindowBound, BottomWindowBound, (Anchor.Y + 1f) * 0.5f);

            if (Mode == ScreenAnchorConstraintMode.XOnly || Mode == ScreenAnchorConstraintMode.BothAxes)
            {
                position.X = targetX + Offset.X;
            }
            if (Mode == ScreenAnchorConstraintMode.YOnly || Mode == ScreenAnchorConstraintMode.BothAxes)
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
            if(Mode == ScreenAnchorConstraintMode.XOnly || Mode == ScreenAnchorConstraintMode.BothAxes)
            {
                if(component.Transform.Position.X != position.X)
                {
                    return false;
                }
            }
            if(Mode == ScreenAnchorConstraintMode.YOnly || Mode == ScreenAnchorConstraintMode.BothAxes)
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
