namespace Electron2D.UI
{
    public class UIContainer : UIElement
    {
        public UIContainer(int sizeX = 0, int sizeY = 0, int uiRenderLayer = 0, bool useScreenPosition = true)
            : base(sizeX, sizeY, uiRenderLayer, useScreenPosition, true, false)
        { }

        public override void UpdateMesh() { }
    }
}
