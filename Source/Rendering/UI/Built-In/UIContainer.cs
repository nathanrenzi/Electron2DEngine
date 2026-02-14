namespace Electron2D.UI
{
    public sealed class UIContainer : UIElement
    {
        public UIContainer(UIElementArgs? arguments = null) : base(arguments, false, true) { }
        public override void UpdateMesh() { }
    }
}
