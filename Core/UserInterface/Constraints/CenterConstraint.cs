using System.Numerics;
using Electron2D.Core.UI;

namespace Electron2D.Core.UserInterface
{
    public class CenterConstraint : UiConstraint
    {
        public CenterConstraint(UiConstraintSide _side) : base() { side = _side; }

        public override void ApplyConstraint(UiComponent _component)
        {
            float x, y;
            x = side == UiConstraintSide.Left || side == UiConstraintSide.Right ? 0 : _component.transform.position.X;
            y = side == UiConstraintSide.Top || side == UiConstraintSide.Bottom ? 0 : _component.transform.position.Y;
            _component.transform.position = new Vector2(x, y);
        }
    }
}
