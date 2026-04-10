using System.Numerics;

namespace Electron2D.UI
{
    /// <summary>
    /// Defines the visual style of a <see cref="UISlider"/>.
    /// </summary>
    public sealed class UISliderStyle
    {
        /// <summary>
        /// The panel definition for the slider background track.
        /// </summary>
        public UIPanelDef BackgroundDef { get; }

        /// <summary>
        /// The panel definition for the filled portion of the track.
        /// </summary>
        public UIPanelDef ForegroundDef { get; }

        /// <summary>
        /// The panel definition for the draggable handle.
        /// </summary>
        public UIPanelDef HandleDef { get; }

        /// <summary>
        /// The margin applied to the background track.
        /// </summary>
        public Border BackgroundMargin { get; }

        /// <summary>
        /// The margin applied to the foreground fill.
        /// </summary>
        public Border ForegroundMargin { get; }

        /// <summary>
        /// The size of the handle in pixels.
        /// </summary>
        public Vector2 HandleSize { get; }

        /// <summary>
        /// The padding at each end of the track that the handle cannot pass.
        /// </summary>
        public int HandleEndPadding { get; }

        /// <summary>
        /// Creates a new <see cref="UISliderStyle"/>.
        /// </summary>
        /// <param name="backgroundDef">The panel definition for the background track.</param>
        /// <param name="foregroundDef">The panel definition for the foreground fill.</param>
        /// <param name="handleDef">The panel definition for the handle.</param>
        /// <param name="handleSize">The size of the handle in pixels.</param>
        /// <param name="backgroundMargin">The margin applied to the background track. Defaults to zero.</param>
        /// <param name="foregroundMargin">The margin applied to the foreground fill. Defaults to zero.</param>
        /// <param name="endPadding">The padding at each end of the track that the handle cannot pass. Defaults to zero.</param>
        public UISliderStyle(UIPanelDef backgroundDef, UIPanelDef foregroundDef,
            UIPanelDef handleDef, Vector2 handleSize, Border? backgroundMargin = null,
            Border? foregroundMargin = null, int endPadding = 0)
        {
            BackgroundDef = backgroundDef ?? throw new ArgumentNullException(nameof(backgroundDef));
            ForegroundDef = foregroundDef ?? throw new ArgumentNullException(nameof(foregroundDef));
            HandleDef = handleDef ?? throw new ArgumentNullException(nameof(handleDef));
            HandleSize = handleSize;
            BackgroundMargin = backgroundMargin ?? new Border(0);
            ForegroundMargin = foregroundMargin ?? new Border(0);
            HandleEndPadding = endPadding;
        }
    }
}
