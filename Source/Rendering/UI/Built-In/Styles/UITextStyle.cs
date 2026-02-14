using Electron2D.Rendering.Shaders;
using Electron2D.Rendering.Text;
using System.Drawing;

namespace Electron2D.UI
{
    public sealed class UITextStyle
    {
        public Color Color { get; }
        public FontArgs FontArguments { get; }
        public TextAlignment HorizontalAlignment { get; }
        public TextAlignment VerticalAlignment { get; }
        public TextAlignmentMode AlignmentMode { get; }
        public TextOverflowMode OverflowMode { get; }
        public float LineHeightMultiplier { get; }
        public Shader? CustomShader { get; }

        public UITextStyle(Color color, FontArgs fontArguments, TextAlignment horizontalAlignment = TextAlignment.Left,
            TextAlignment verticalAlignment = TextAlignment.Top, TextAlignmentMode alignmentMode = TextAlignmentMode.Baseline,
            TextOverflowMode overflowMode = TextOverflowMode.Word, float lineHeightMultiplier = 1.35f, Shader? customShader = null)
        {
            Color = color;
            FontArguments = fontArguments;
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            AlignmentMode = alignmentMode;
            OverflowMode = overflowMode;
            LineHeightMultiplier = lineHeightMultiplier;
            CustomShader = customShader;
        }
    }
}
