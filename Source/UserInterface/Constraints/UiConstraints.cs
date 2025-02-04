namespace Electron2D.UserInterface
{
    public class UIConstraints
    {
        private UIConstraint positionXConstraint;
        private UIConstraint positionYConstraint;
        private UIConstraint sizeXConstraint;
        private UIConstraint sizeYConstraint;
        private UIComponent component;

        public UIConstraints(UIComponent _component)
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

        public void SetPosition(UIConstraint _constraint)
        {
            if (_constraint.side == UIConstraintSide.Left || _constraint.side == UIConstraintSide.Right)
            {
                positionXConstraint = _constraint;
            }
            else if (_constraint.side == UIConstraintSide.Top || _constraint.side == UIConstraintSide.Bottom)
            {
                positionYConstraint = _constraint;
            }
            _constraint.ApplyConstraint(component);
        }

        public void SetSize(UIConstraint _constraint)
        {
            if(_constraint.side == UIConstraintSide.Left || _constraint.side == UIConstraintSide.Right)
            {
                sizeXConstraint = _constraint;
            }
            else if (_constraint.side == UIConstraintSide.Top || _constraint.side == UIConstraintSide.Bottom)
            {
                sizeYConstraint = _constraint;
            }
            _constraint.ApplyConstraint(component);
        }
    }
}
