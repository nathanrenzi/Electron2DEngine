using System.Numerics;

namespace Electron2D.UserInterface
{
    public class PixelConstraint : UIConstraint
    {
        private int _pixelDistance;

        public PixelConstraint(int pixelDistance, UIConstraintSide side)
        {
            _pixelDistance = pixelDistance;
            Side = side;
        }

        public override void ApplyConstraint(UIComponent _component)
        {
            switch (Side)
            {
                case UIConstraintSide.Left:
                    _component.Transform.Position = new Vector2(LeftWindowBound + _pixelDistance + MathF.Abs(_component.LeftXBound), _component.Transform.Position.Y);
                    break;

                case UIConstraintSide.Right:
                    _component.Transform.Position = new Vector2(RightWindowBound - _pixelDistance - MathF.Abs(_component.RightXBound), _component.Transform.Position.Y);
                    break;

                case UIConstraintSide.Top:
                    _component.Transform.Position = new Vector2(_component.Transform.Position.X, TopWindowBound + _pixelDistance + MathF.Abs(_component.TopYBound));
                    break;

                case UIConstraintSide.Bottom:
                    _component.Transform.Position = new Vector2(_component.Transform.Position.X, BottomWindowBound - _pixelDistance - MathF.Abs(_component.BottomYBound));
                    break;
            }
        }
    }
}