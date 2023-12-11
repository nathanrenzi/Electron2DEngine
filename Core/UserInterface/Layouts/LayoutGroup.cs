using Electron2D.Core.ECS;
using Electron2D.Core.UI;

namespace Electron2D.Core.UserInterface
{
    public abstract class LayoutGroup : UiListener
    {
        public List<UiComponent> components = new List<UiComponent>();
        private bool active = false;

        protected UiComponent parent;

        public void OnUiAction(object _sender, UiEvent _event)
        {
            if(_event == UiEvent.Resize)
            {
                RecalculateLayout();
            }
        }

        public void SetUiParent(UiComponent _parent)
        {
            if(parent != null)
            {
                parent.RemoveUiListener(this);
            }

            parent = _parent;
            parent.AddUiListener(this);
            active = true;
        }

        public void AddToLayout(UiComponent _component, bool _recalculateLayout = true)
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
            if(_recalculateLayout) RecalculateLayout();
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

        public abstract void RecalculateLayout();
    }
}
