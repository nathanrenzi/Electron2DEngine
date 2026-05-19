namespace Atlas2D.UI
{
    /// <summary>
    /// Arguments that control how a <see cref="UIElement"/> is rendered.
    /// </summary>
    public struct UIRenderArgs
    {
        /// <summary>
        /// The render layer this element is drawn on.
        /// </summary>
        public int RenderLayer;

        /// <summary>
        /// Whether this element is positioned in world space rather than screen space.
        /// </summary>
        public bool UseWorldPosition;

        /// <summary>
        /// Whether this element is excluded from post-processing effects.
        /// </summary>
        public bool IgnorePostProcessing;

        /// <summary>
        /// Whether this element masks its children to its bounds.
        /// </summary>
        public bool Mask;

        /// <summary>
        /// Whether this element's position is snapped to the nearest pixel.
        /// </summary>
        public bool SnapToPixels;

        /// <summary>
        /// Creates a new <see cref="UIRenderArgs"/> with default values.
        /// </summary>
        public UIRenderArgs()
        {
            RenderLayer = 0;
            UseWorldPosition = false;
            IgnorePostProcessing = true;
            Mask = false;
            SnapToPixels = true;
        }

        /// <summary>
        /// Creates a new <see cref="UIRenderArgs"/> copied from an existing instance.
        /// </summary>
        /// <param name="argsToCopy">The <see cref="UIRenderArgs"/> to copy values from.</param>
        public UIRenderArgs(UIRenderArgs argsToCopy)
        {
            RenderLayer = argsToCopy.RenderLayer;
            UseWorldPosition = argsToCopy.UseWorldPosition;
            IgnorePostProcessing = argsToCopy.IgnorePostProcessing;
            Mask = argsToCopy.Mask;
            SnapToPixels = argsToCopy.SnapToPixels;
        }
    }
}
