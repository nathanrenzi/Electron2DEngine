namespace Atlas2D.UI
{
    public enum ScrollBarVisibility
    {
        /// <summary>
        /// Visible only when content overflows.
        /// </summary>
        Auto,

        /// <summary>
        /// Always visible.
        /// </summary>
        Always,

        /// <summary>
        /// Never visible (not created).
        /// </summary>
        Never
    }

    public sealed class UIScrollContainerStyle
    {
        /// <summary>
        /// Controls when the vertical scrollbar is shown.
        /// </summary>
        public ScrollBarVisibility VerticalVisibility { get; }

        /// <summary>
        /// Controls when the horizontal scrollbar is shown.
        /// </summary>
        public ScrollBarVisibility HorizontalVisibility { get; }

        /// <summary>
        /// The panel definition for the scrollbar handle.
        /// </summary>
        public UIPanelDef HandleDef { get; }

        /// <summary>
        /// The panel definition for the scrollbar track, or <see langword="null"> for no track.
        /// </summary>
        public UIPanelDef? BackgroundDef { get; }

        /// <summary>
        /// The thickness of the scrollbar in pixels.
        /// </summary>
        public float Thickness { get; }

        /// <summary>
        /// The minimum length of the handle in pixels.
        /// </summary>
        public float MinHandleSize { get; }

        /// <summary>
        /// Creates a new <see cref="UIScrollContainerStyle"/>.
        /// </summary>
        /// <param name="handleDef">The panel definition for the scrollbar handle.</param>
        /// <param name="backgroundDef">The panel definition for the scrollbar track, or <see langword="null"/> for no track.</param>
        /// <param name="thickness">The thickness of the scrollbar in pixels. Defaults to 8.</param>
        /// <param name="minHandleSize">The minimum length of the handle in pixels. Defaults to 16.</param>
        /// <param name="verticalVisibility">Controls when the vertical scrollbar is shown. Defaults to <see cref="ScrollBarVisibility.Auto"/>.</param>
        /// <param name="horizontalVisibility">Controls when the horizontal scrollbar is shown. Defaults to <see cref="ScrollBarVisibility.Auto"/>.</param>
        public UIScrollContainerStyle(UIPanelDef handleDef, UIPanelDef? backgroundDef = null, float thickness = 8f,
            float minHandleSize = 16f, ScrollBarVisibility verticalVisibility = ScrollBarVisibility.Auto,
            ScrollBarVisibility horizontalVisibility = ScrollBarVisibility.Auto)
        {
            HandleDef = handleDef;
            BackgroundDef = backgroundDef;
            Thickness = thickness;
            MinHandleSize = minHandleSize;
            VerticalVisibility = verticalVisibility;
            HorizontalVisibility = horizontalVisibility;
        }
    }
}
