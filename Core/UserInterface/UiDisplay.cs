namespace Electron2D.Core.UI
{
    public class UiDisplay
    {
        private SortedList<int, List<UiComponent>> sortedActiveComponents = new SortedList<int, List<UiComponent>>();
        private List<UiComponent> activeComponents = new List<UiComponent>();
        // Add an ActiveUiComponent class that also holds a reference to the constraints being used
        //  and use that for the above sorted list instead

        public void RegisterUiComponent(UiComponent _component/*, UiConstraint _constraint*/)
        {
            if (activeComponents.Contains(_component)) return;

            OrderUiComponent(_component);
            activeComponents.Add(_component);
        }

        public void UnregisterUiComponent(UiComponent _component)
        {
            if (!activeComponents.Contains(_component)) return;

            activeComponents.Remove(_component);

            // Removing the object from the render layer dictionary
            List<UiComponent> list;
            if (sortedActiveComponents.TryGetValue(_component.uiRenderLayer, out list))
            {
                list.Remove(_component);
            }
        }

        public void OrderUiComponent(UiComponent _component, bool _reorder = false, int _oldRenderLayer = -1, int _newRenderLayer = -1)
        {
            // Removing the old render layer if the component is reordering itself instead of initializing
            if (_reorder)
            {
                // If the render layer is registered in the sorted list, remove the ui component from the value list
                List<UiComponent> list;
                if (sortedActiveComponents.TryGetValue(_oldRenderLayer, out list))
                {
                    bool removed = list.Remove(_component);
                    if (!removed) Console.WriteLine($"Since item does not exist in layer {_oldRenderLayer}, cannot remove it.");
                }
            }

            int renderOrder = _reorder ? _newRenderLayer : _component.uiRenderLayer;
            // If true, the render layer was not in the sorted list yet so it is added
            if (!sortedActiveComponents.TryAdd(renderOrder, new List<UiComponent> { _component }))
            {
                // If false, the render layer already exists so the object must be added to an existing value list
                sortedActiveComponents[renderOrder].Add(_component);

                // The sorted list class is already sorted in ascending layer, so no extra sorting is necessary
            }
        }


        // Add a mouse checker that goes through the bounds of every component and sends any events
    }
}