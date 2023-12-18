using Electron2D.Core.ECS;
using Electron2D.Core.UI;

namespace Electron2D.Core.UserInterface
{
    public abstract class LayoutGroup : UiListener
    {
        public List<UiComponent> components = new List<UiComponent>();
        private bool active = false;
        private bool isDirty = false;
        private bool addedToUpdateLoop = false;

        protected UiComponent parent;

        public void OnUiAction(object _sender, UiEvent _event)
        {
            if(_event == UiEvent.Resize)
            {
                isDirty = true;
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
                Game.OnUpdateEvent += Game_OnUpdateEvent;
            }

            parent = _parent;
            parent.AddUiListener(this);
            active = true;
        }

        private void Game_OnUpdateEvent()
        {
            if(isDirty)
            {
                isDirty = false;
                RecalculateLayout();
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

            isDirty = true;
            components.Add(_component);
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
                RecalculateLayout();
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
