using System.Numerics;

namespace Electron2D.UI
{
    public sealed class UIContainer : UIElement
    {
        public UIContainer(UIRenderArgs? arguments = null) : base(arguments, false) { }
        public override void UpdateMesh() { }
    }
}
