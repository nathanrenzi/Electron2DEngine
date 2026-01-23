namespace Electron2D.UserInterface
{
    public abstract class LayoutGroup : UIListener, IGameClass
    {
        public List<UIComponent> Components = new List<UIComponent>();
        private bool _active = false;
        private bool _isDirty = false;
        private bool _registeredToGameLoop = false;
        protected UIComponent _parent;

        ~LayoutGroup()
        {
            Dispose();
        }

        public void FixedUpdate() { }

        public void Dispose()
        {
            Engine.Game.UnregisterGameClass(this);
            GC.SuppressFinalize(this);
        }

        private void FlagIsDirty()
        {
            _isDirty = true;
            if (_parent.ParentLayoutGroup != null)
                _parent.ParentLayoutGroup.FlagIsDirty();
        }

        public void OnUIAction(object sender, UIEvent uiEvent)
        {
            if(uiEvent == UIEvent.Resize)
            {
                FlagIsDirty();
            }
        }

        public void SetUIParent(UIComponent parent)
        {
            if(_parent != null)
            {
                _parent.RemoveUIListener(this);
            }

            if(!_registeredToGameLoop)
            {
                Engine.Game.RegisterGameClass(this);
                _registeredToGameLoop = true;
            }

            _parent = parent;
            _parent.AddUIListener(this);
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
                    if (Components[i].LayoutGroup != null)
                    {
                        Components[i].LayoutGroup._isDirty = true;
                    }
                }
            }
        }

        public void AddToLayout(UIComponent _component, bool _updateRenderLayer = true)
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

            if (_updateRenderLayer)
                _component.SetRenderLayer(_parent.UIRenderLayer + 1);

            _component.SetParentLayoutGroup(this);
            Components.Add(_component);
            FlagIsDirty();
        }

        public bool RemoveFromLayout(UIComponent _component)
        {
            if (!_active)
            {
                Debug.LogError("UI LAYOUT: Trying to add UI component to invalid layout object.");
                return false;
            }

            if (Components.Contains(_component))
            {
                Components.Remove(_component);
                FlagIsDirty();
                _component.SetParentLayoutGroup(null);
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
