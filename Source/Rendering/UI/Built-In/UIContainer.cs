using System.Numerics;

namespace Electron2D.UI
{
    public sealed class UIContainer : UIElement
    {
        public UIContainer(UIRenderArgs? arguments = null) : base(arguments, false, true) { Size = new Vector2(100); }
        public override void UpdateMesh() { }
    }
}
