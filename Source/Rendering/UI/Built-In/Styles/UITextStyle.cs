using Electron2D.Rendering.Text;
using System.Drawing;

namespace Electron2D.UI
{
    public sealed class UITextStyle
    {
        public Color Color { get; }
        public FontArguments FontArguments { get; }
        public TextAlignment HorizontalAlignment { get; }
        public TextAlignment VerticalAlignment { get; }
        public TextAlignmentMode AlignmentMode { get; }
        public TextOverflowMode OverflowMode { get; }
        public float LineHeightMultiplier { get; }

        public UITextStyle(Color color, FontArguments fontArguments, TextAlignment horizontalAlignment = TextAlignment.Left, TextAlignment verticalAlignment = TextAlignment.Top,
            TextAlignmentMode alignmentMode = TextAlignmentMode.Baseline, TextOverflowMode overflowMode = TextOverflowMode.Word, float lineHeightMultiplier = 1.35f)
        {
            Color = color;
            FontArguments = fontArguments;
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            AlignmentMode = alignmentMode;
            OverflowMode = overflowMode;
            LineHeightMultiplier = lineHeightMultiplier;
        }
    }
}
