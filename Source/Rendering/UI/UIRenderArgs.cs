namespace Electron2D.UI
{
    public struct UIRenderArgs
    {
        public int RenderLayer;
        public bool UseWorldPosition;
        public bool IgnorePostProcessing;
        public bool Mask;

        public UIRenderArgs()
        {
            RenderLayer = 0;
            UseWorldPosition = false;
            IgnorePostProcessing = true;
            Mask = false;
        }

        public UIRenderArgs(UIRenderArgs argsToCopy)
        {
            RenderLayer = argsToCopy.RenderLayer;
            UseWorldPosition = argsToCopy.UseWorldPosition;
            IgnorePostProcessing = argsToCopy.IgnorePostProcessing;
            Mask = argsToCopy.Mask;
        }
    }
}
