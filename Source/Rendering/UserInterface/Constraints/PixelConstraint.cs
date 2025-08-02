using System.Numerics;

namespace Electron2D.UserInterface
{
    public class PixelConstraint : UIConstraint
    {
        private int pixelDistance;

        public PixelConstraint(int _pixelDistance, UIConstraintSide _side)
        {
            pixelDistance = _pixelDistance;
            side = _side;
        }

        public override void ApplyConstraint(UIComponent _component)
        {
            switch(side)
            {
                case UIConstraintSide.Left:
                    _component.Transform.Position = new Vector2(LeftWindowBound + pixelDistance + MathF.Abs(_component.LeftXBound), _component.Transform.Position.Y);
                    break;
                case UIConstraintSide.Right:
                    _component.Transform.Position = new Vector2(RightWindowBound - pixelDistance - MathF.Abs(_component.RightXBound), _component.Transform.Position.Y);
                    break;
                case UIConstraintSide.Top:
                    _component.Transform.Position = new Vector2(_component.Transform.Position.X, TopWindowBound - pixelDistance - MathF.Abs(_component.TopYBound));
                    break;
                case UIConstraintSide.Bottom:
                    _component.Transform.Position = new Vector2(_component.Transform.Position.X, BottomWindowBound + pixelDistance + MathF.Abs(_component.BottomYBound));
                    break;
            }
        }
    }
}
