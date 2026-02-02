using System.Numerics;

namespace Electron2D.UI
{
    public interface ILayout
    {
        Vector2 Measure(UIElement parent, Vector2 availableSize);
        void Arrange(UIElement parent, Rect finalRect);
    }
}
