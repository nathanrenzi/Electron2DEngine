

namespace Electron2D.UI
{
    /// <summary>
    /// Defines per-state colors for a single layer of a <see cref="UIButton"/> (background, text, or icon).
    /// </summary>
    public sealed class UIButtonLayerStyle
    {
        /// <summary>The color when the button is in its normal state.</summary>
        public Color Normal { get; set; }

        /// <summary>The color when the button is hovered.</summary>
        public Color Hover { get; set; }

        /// <summary>The color when the button is pressed.</summary>
        public Color Pressed { get; set; }

        /// <summary>The color when the button is disabled.</summary>
        public Color Disabled { get; set; }

        /// <summary>
        /// Creates a layer style with an explicit color for each state.
        /// </summary>
        public UIButtonLayerStyle(Color normal, Color hover, Color pressed, Color disabled)
        {
            Normal = normal;
            Hover = hover;
            Pressed = pressed;
            Disabled = disabled;
        }

        /// <summary>
        /// Creates a layer style where all states share the same color.
        /// </summary>
        public UIButtonLayerStyle(Color color)
            : this(color, color, color, color) { }

        /// <summary>
        /// Returns the appropriate color for the given button state flags.
        /// </summary>
        public Color Resolve(bool isDisabled, bool isPressed, bool isHovered)
        {
            if (isDisabled) return Disabled;
            if (isPressed) return Pressed;
            if (isHovered) return Hover;
            return Normal;
        }
    }


    /// <summary>
    /// Combines layer styles for the background, text, and icon of a <see cref="UIButton"/>.
    /// Any layer can be <see langword="null"/> to indicate that layer should not have its color managed by the style.
    /// </summary>
    public sealed class UIButtonStyle
    {
        /// <summary>Per-state colors for the button background. Null to leave unmanaged.</summary>
        public UIButtonLayerStyle? Background { get; set; }

        /// <summary>Per-state colors for the button text. Null to leave unmanaged.</summary>
        public UIButtonLayerStyle? Text { get; set; }

        /// <summary>Per-state colors for the button icon. Null to leave unmanaged.</summary>
        public UIButtonLayerStyle? Icon { get; set; }

        public UIButtonStyle(
            UIButtonLayerStyle? background = null,
            UIButtonLayerStyle? text = null,
            UIButtonLayerStyle? icon = null)
        {
            Background = background;
            Text = text;
            Icon = icon;
        }

        /// <summary>
        /// Creates a simple flat style from a single normal background color and foreground color,
        /// with no hover/pressed differentiation. Useful as a quick default.
        /// </summary>
        public static UIButtonStyle Flat(Color background, Color foreground)
        {
            var bg = new UIButtonLayerStyle(background);
            var fg = new UIButtonLayerStyle(foreground);
            return new UIButtonStyle(background: bg, text: fg, icon: fg);
        }
    }
}
