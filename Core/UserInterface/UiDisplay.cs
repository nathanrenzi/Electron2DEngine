namespace Electron2D.Core.UI
{
    public class UiDisplay
    {
        private List<UiComponent> activeComponents = new List<UiComponent>();
        // Add an ActiveUiComponent class that also holds a reference to the constraints being used
        //  and use that for the above list instead

        public void RegisterUiComponent(UiComponent _component/*, UiConstraint _constraint*/)
        {
            if (activeComponents.Contains(_component)) return;
            activeComponents.Add(_component);
        }

        public void UnregisterUiComponent(UiComponent _component)
        {
            if (!activeComponents.Contains(_component)) return;
            activeComponents.Remove(_component);
        }


        // Add a mouse checker that goes through the bounds of every component and sends any events
    }
}