namespace Electron2D.UI
{
    public struct UIRenderArgs
    {
        public int RenderLayer;
        public bool UseScreenPosition;
        public bool IgnorePostProcessing;

        public UIRenderArgs()
        {
            RenderLayer = 0;
            UseScreenPosition = true;
            IgnorePostProcessing = true;
        }

        public UIRenderArgs(UIRenderArgs argsToCopy)
        {
            RenderLayer = argsToCopy.RenderLayer;
            UseScreenPosition = argsToCopy.UseScreenPosition;
            IgnorePostProcessing = argsToCopy.IgnorePostProcessing;
        }
    }
}
