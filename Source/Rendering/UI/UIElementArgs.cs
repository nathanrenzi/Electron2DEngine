namespace Electron2D.UI
{
    public struct UIElementArgs
    {
        public int SizeX;
        public int SizeY;
        public int RenderLayer;
        public bool UseScreenPosition;
        public bool IgnorePostProcessing;

        public UIElementArgs()
        {
            SizeX = 0;
            SizeY = 0;
            RenderLayer = 0;
            UseScreenPosition = true;
            IgnorePostProcessing = true;
        }

        public UIElementArgs(UIElementArgs argsToCopy)
        {
            SizeX = argsToCopy.SizeX;
            SizeY = argsToCopy.SizeY;
            RenderLayer = argsToCopy.RenderLayer;
            UseScreenPosition = argsToCopy.UseScreenPosition;
            IgnorePostProcessing = argsToCopy.IgnorePostProcessing;
        }
    }
}
