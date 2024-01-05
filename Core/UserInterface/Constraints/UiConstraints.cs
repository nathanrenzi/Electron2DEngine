using Electron2D.Core.UserInterface;

namespace Electron2D.Core.UserInterface
{
    public class UiConstraints
    {
        private UiConstraint positionXConstraint;
        private UiConstraint positionYConstraint;
        private UiConstraint sizeXConstraint;
        private UiConstraint sizeYConstraint;
        private UiComponent component;

        public UiConstraints(UiComponent _component)
        {
            component = _component;
        }

        public void ApplyConstraints()
        {
            if(sizeXConstraint != null) sizeXConstraint.ApplyConstraint(component);
            if (sizeYConstraint != null) sizeYConstraint.ApplyConstraint(component);
            if (positionXConstraint != null) positionXConstraint.ApplyConstraint(component);
            if (positionYConstraint != null) positionYConstraint.ApplyConstraint(component);
        }

        public void SetPosition(UiConstraint _constraint)
        {
            if (_constraint.side == UiConstraintSide.Left || _constraint.side == UiConstraintSide.Right)
            {
                positionXConstraint = _constraint;
            }
            else if (_constraint.side == UiConstraintSide.Top || _constraint.side == UiConstraintSide.Bottom)
            {
                positionYConstraint = _constraint;
            }
            _constraint.ApplyConstraint(component);
        }

        public void SetSize(UiConstraint _constraint)
        {
            if(_constraint.side == UiConstraintSide.Left || _constraint.side == UiConstraintSide.Right)
            {
                sizeXConstraint = _constraint;
            }
            else if (_constraint.side == UiConstraintSide.Top || _constraint.side == UiConstraintSide.Bottom)
            {
                sizeYConstraint = _constraint;
            }
            _constraint.ApplyConstraint(component);
        }
    }
}
