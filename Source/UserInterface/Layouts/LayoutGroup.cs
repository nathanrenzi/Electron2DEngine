namespace Electron2D.UserInterface
{
    public abstract class LayoutGroup : UiListener
    {
        public List<UiComponent> components = new List<UiComponent>();
        private bool active = false;
        private bool IsDirty = false;
        private bool addedToUpdateLoop = false;

        protected UiComponent parent;

        public void OnUiAction(object _sender, UiEvent _event)
        {
            if(_event == UiEvent.Resize)
            {
                IsDirty = true;
            }
        }

        public void SetUiParent(UiComponent _parent)
        {
            if(parent != null)
            {
                parent.RemoveUiListener(this);
            }

            if(!addedToUpdateLoop)
            {
                addedToUpdateLoop = true;
                Game.UpdateEvent += Game_OnUpdateEvent;
            }

            parent = _parent;
            parent.AddUiListener(this);
            active = true;
        }

        private void Game_OnUpdateEvent()
        {
            if(IsDirty)
            {
                IsDirty = false;
                RecalculateLayout();
                for (int i = 0; i < components.Count; i++)
                {
                    if (components[i].ChildLayoutGroup != null)
                    {
                        components[i].ChildLayoutGroup.IsDirty = true;
                    }
                }
            }
        }

        public void AddToLayout(UiComponent _component)
        {
            if(!active)
            {
                Debug.LogError("UI LAYOUT: Trying to add UI component to invalid layout object.");
                return;
            }

            if(components.Contains(_component))
            {
                Debug.LogError("UI LAYOUT: Trying to add UI component when it has already been added to this layout.");
                return;
            }

            components.Add(_component);
            IsDirty = true;
        }

        public bool RemoveFromLayout(UiComponent _component)
        {
            if (!active)
            {
                Debug.LogError("UI LAYOUT: Trying to add UI component to invalid layout object.");
                return false;
            }

            if (components.Contains(_component))
            {
                components.Remove(_component);
                IsDirty = true;
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
