namespace Electron2D.UserInterface
{
    public abstract class LayoutGroup : UiListener, IGameClass
    {
        public List<UiComponent> Components = new List<UiComponent>();
        private bool _active = false;
        private bool _isDirty = false;
        private bool _registeredToGameLoop = false;
        protected UiComponent _parent;

        ~LayoutGroup()
        {
            Dispose();
        }

        public void FixedUpdate() { }

        public void Dispose()
        {
            Program.Game.UnregisterGameClass(this);
            GC.SuppressFinalize(this);
        }

        public void OnUiAction(object sender, UiEvent uiEvent)
        {
            if(uiEvent == UiEvent.Resize)
            {
                _isDirty = true;
            }
        }

        public void SetUiParent(UiComponent parent)
        {
            if(_parent != null)
            {
                _parent.RemoveUiListener(this);
            }

            if(!_registeredToGameLoop)
            {
                Program.Game.RegisterGameClass(this);
                _registeredToGameLoop = true;
            }

            _parent = parent;
            _parent.AddUiListener(this);
            _active = true;
        }

        public void Update()
        {
            if(_isDirty)
            {
                _isDirty = false;
                RecalculateLayout();
                for (int i = 0; i < Components.Count; i++)
                {
                    if (Components[i].ChildLayoutGroup != null)
                    {
                        Components[i].ChildLayoutGroup._isDirty = true;
                    }
                }
            }
        }

        public void AddToLayout(UiComponent _component)
        {
            if(!_active)
            {
                Debug.LogError("UI LAYOUT: Trying to add UI component to invalid layout object.");
                return;
            }

            if(Components.Contains(_component))
            {
                Debug.LogError("UI LAYOUT: Trying to add UI component when it has already been added to this layout.");
                return;
            }

            Components.Add(_component);
            _isDirty = true;
        }

        public bool RemoveFromLayout(UiComponent _component)
        {
            if (!_active)
            {
                Debug.LogError("UI LAYOUT: Trying to add UI component to invalid layout object.");
                return false;
            }

            if (Components.Contains(_component))
            {
                Components.Remove(_component);
                _isDirty = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected abstract void RecalculateLayout();
    }
}
