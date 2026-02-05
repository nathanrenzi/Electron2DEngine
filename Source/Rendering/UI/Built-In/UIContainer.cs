namespace Electron2D.UI
{
    public sealed class UIContainer : UIElement
    {
        public UIContainer(int sizeX = 0, int sizeY = 0, int renderLayer = 0, bool useScreenPosition = true)
            : base(sizeX, sizeY, renderLayer, useScreenPosition, true, false)
        { }

        public override void UpdateMesh() { }
    }
}
