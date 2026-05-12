using System.Numerics;

namespace Electron2D.UI
{
    /// <summary>
    /// Defines the base class for UI layout strategies that control how child elements are measured and arranged.
    /// </summary>
    public abstract class UILayout
    {
        private List<UIElement> _registeredElements = new List<UIElement>();

        internal void RegisterElement(UIElement element)
        {
            if(!_registeredElements.Contains(element))
                _registeredElements.Add(element);
        }

        internal void UnregisterElement(UIElement element)
        {
            _registeredElements.Remove(element);
        }

        internal abstract Vector2 Measure(UIElement parent, Vector2 availableSize);
        internal abstract void Arrange(UIElement parent, Rect finalRect);

        protected void InvalidateMeasure()
        {
            foreach(UIElement element in _registeredElements)
            {
                element.InvalidateMeasure();
            }
        }

        protected void InvalidateArrange()
        {
            foreach (UIElement element in _registeredElements)
            {
                element.InvalidateArrange();
            }
        }
    }
}
