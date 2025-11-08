namespace Electron2D.UserInterface
{
    public class UIConstraints
    {
        public bool IsDirty { get; set; }
        private List<UIConstraint> _constraints = new List<UIConstraint>();
        private UIComponent _component;

        public UIConstraints(UIComponent component)
        {
            _component = component;
        }

        public void Add(UIConstraint constraint)
        {
            if(!_constraints.Contains(constraint))
            {
                _constraints.Add(constraint);
                IsDirty = true;
            }
            else
            {
                Debug.LogError("Constraint has already been added to this UIConstraints object, cannot add!");
            }
        }

        public void Remove(UIConstraint constraint)
        {
            _constraints.Remove(constraint);
            IsDirty = true;
        }

        public void ApplyConstraints()
        {
            foreach (var constraint in _constraints)
            {
                constraint.ApplyConstraint(_component);
            }

            foreach (var constraint in _constraints)
            {
                if(!constraint.CheckConstraint(_component))
                {
                    Debug.LogWarning($"Constraint of type '{constraint.GetType().Name}' was not satisfied!");
                }
            }
        }

        public void Clear()
        {
            _constraints.Clear();
        }
    }
}
