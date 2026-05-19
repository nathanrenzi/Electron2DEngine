using System.Numerics;

namespace Atlas2D.UI
{
    /// <summary>
    /// Defines how the UI canvas is scaled relative to the window size.
    /// </summary>
    public enum UIScalingMode
    {
        /// <summary>
        /// The UI uses the actual window resolution with no scaling applied.
        /// </summary>
        RealResolution,

        /// <summary>
        /// The UI is rendered at a fixed virtual resolution and scaled to fit the window.
        /// </summary>
        VirtualResolution
    }

    /// <summary>
    /// Defines the scaling and resolution settings for the <see cref="UICanvas"/>.
    /// </summary>
    public sealed class UICanvasSettings
    {
        /// <summary>
        /// The scaling mode used to map the UI to the window. Defaults to <see cref="UIScalingMode.VirtualResolution"/>.
        /// </summary>
        public UIScalingMode ScalingMode = UIScalingMode.VirtualResolution;

        /// <summary>
        /// The virtual resolution the UI is designed for, in pixels. Only used when <see cref="ScalingMode"/> is <see cref="UIScalingMode.VirtualResolution"/>. Defaults to 1920x1080.
        /// </summary>
        public Vector2 VirtualResolution = new Vector2(1920, 1080);

        /// <summary>
        /// Whether to maintain the aspect ratio of <see cref="VirtualResolution"/> when scaling to fit the window, adding letterboxing if needed. Defaults to true.
        /// </summary>
        public bool MaintainAspect = true;
    }
}
