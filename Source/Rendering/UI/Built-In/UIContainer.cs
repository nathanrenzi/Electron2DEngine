using System.Numerics;

namespace Atlas2D.UI
{
    /// <summary>
    /// A blank, invisible UI element used for grouping or layout purposes.
    /// </summary>
    public sealed class UIContainer(UIRenderArgs? arguments = null) : UIElement(arguments, false)
    {
        public override void UpdateMesh() { }
    }
}
