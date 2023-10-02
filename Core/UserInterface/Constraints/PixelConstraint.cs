using Electron2D.Core.UI;
using System.Numerics;

namespace Electron2D.Core.UserInterface
{
    public class PixelConstraint : UiConstraint
    {
        private int pixelDistance;

        public PixelConstraint(int _pixelDistance, UiConstraintSide _side)
        {
            pixelDistance = _pixelDistance;
            side = _side;
        }

        public override void ApplyConstraint(UiComponent _component)
        {
            switch(side)
            {
                case UiConstraintSide.Left:
                    _component.Transform.Position = new Vector2(LeftWindowBound + pixelDistance + MathF.Abs(_component.LeftXBound), _component.Transform.Position.Y);
                    break;
                case UiConstraintSide.Right:
                    _component.Transform.Position = new Vector2(RightWindowBound - pixelDistance - MathF.Abs(_component.RightXBound), _component.Transform.Position.Y);
                    break;
                case UiConstraintSide.Top:
                    _component.Transform.Position = new Vector2(_component.Transform.Position.X, TopWindowBound - pixelDistance - MathF.Abs(_component.TopYBound));
                    break;
                case UiConstraintSide.Bottom:
                    _component.Transform.Position = new Vector2(_component.Transform.Position.X, BottomWindowBound + pixelDistance + MathF.Abs(_component.BottomYBound));
                    break;
            }
        }
    }
}
