using Electron2D.Rendering.Shaders;
using Electron2D.Rendering.Text;
using System.Drawing;

namespace Electron2D.UI
{
    /// <summary>
    /// Defines the visual style of a <see cref="UIText"/>.
    /// </summary>
    public sealed class UITextStyle
    {
        /// <summary>
        /// The color of the text.
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// The font and size arguments used to render the text.
        /// </summary>
        public FontArgs FontArguments { get; }

        /// <summary>
        /// The horizontal alignment of the text.
        /// </summary>
        public TextAlignment HorizontalAlignment { get; }

        /// <summary>
        /// The vertical alignment of the text.
        /// </summary>
        public TextAlignment VerticalAlignment { get; }

        /// <summary>
        /// How text is handled when it overflows the available area.
        /// </summary>
        public TextOverflowMode OverflowMode { get; }

        /// <summary>
        /// The line height multiplier applied to the font's default line height.
        /// </summary>
        public float LineHeightMultiplier { get; }

        /// <summary>
        /// A custom shader used to render the text, or <see langword="null"/> to use the default.
        /// </summary>
        public Shader? CustomShader { get; }

        /// <summary>
        /// Creates a new <see cref="UITextStyle"/>.
        /// </summary>
        /// <param name="color">The color of the text.</param>
        /// <param name="fontArguments">The font and size arguments used to render the text.</param>
        /// <param name="horizontalAlignment">The horizontal alignment of the text. Defaults to <see cref="TextAlignment.Left"/>.</param>
        /// <param name="verticalAlignment">The vertical alignment of the text. Defaults to <see cref="TextAlignment.Top"/>.</param>
        /// <param name="overflowMode">How text is handled when it overflows. Defaults to <see cref="TextOverflowMode.Word"/>.</param>
        /// <param name="lineHeightMultiplier">The line height multiplier. Defaults to 1.35.</param>
        /// <param name="customShader">A custom shader for rendering the text. Defaults to <see langword="null"/>.</param>
        public UITextStyle(Color color, FontArgs fontArguments, TextAlignment horizontalAlignment = TextAlignment.Left,
            TextAlignment verticalAlignment = TextAlignment.Top, TextOverflowMode overflowMode = TextOverflowMode.Word,
            float lineHeightMultiplier = 1.35f, Shader? customShader = null)
        {
            Color = color;
            FontArguments = fontArguments;
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            OverflowMode = overflowMode;
            LineHeightMultiplier = lineHeightMultiplier;
            CustomShader = customShader;
        }
    }
}