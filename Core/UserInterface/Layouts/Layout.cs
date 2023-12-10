using Electron2D.Core.ECS;
using Electron2D.Core.UI;

namespace Electron2D.Core.UserInterface
{
    public class LayoutSystem : BaseSystem<Layout> { }
    public abstract class Layout : Component, UiListener
    {
        protected List<UiComponent> components = new List<UiComponent>();
        private bool passedTypeCheck = false;

        protected UiComponent parent;

        public Layout()
        {
            LayoutSystem.Register(this);
        }

        public override void OnComponentAdded()
        {
            if(Entity is not UiComponent)
            {
                Debug.LogError("UI LAYOUT: Layout object added to non UI Component. This is not allowed.");
                return;
            }

            parent = (UiComponent)Entity;
            parent.AddUiListener(this);
            passedTypeCheck = true;
        }

        public void OnUiAction(object _sender, UiEvent _event)
        {
            if(_event == UiEvent.Resize)
            {
                RecalculateLayout();
            }
        }

        public void AddToLayout(UiComponent _component)
        {
            if(!passedTypeCheck)
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
            RecalculateLayout();
        }

        public bool RemoveFromLayout(UiComponent _component)
        {
            if (!passedTypeCheck)
            {
                Debug.LogError("UI LAYOUT: Trying to add UI component to invalid layout object.");
                return false;
            }

            if (components.Contains(_component))
            {
                components.Remove(_component);
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
