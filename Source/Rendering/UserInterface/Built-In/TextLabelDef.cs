using Electron2D.Rendering;
using Electron2D.Rendering.Text;
using System.Drawing;
using System.Numerics;

namespace Electron2D.UserInterface
{
    public class TextLabelDef
    {
        public float SizeX;
        public float SizeY;
        public Color TextColor;
        public TextAlignment TextHorizontalAlignment;
        public TextAlignment TextVerticalAlignment;
        public TextAlignmentMode TextAlignmentMode;
        public TextOverflowMode TextOverflowMode;
        public FontArguments TextFontArguments;
        public Material TextMaterial;
        public string Text;

        /// <summary>
        /// Creates a new definition for a text label.
        /// </summary>
        /// <param name="sizeX">The size of the field on the X axis.</param>
        /// <param name="sizeY">The size of the field on the Y axis.</param>
        /// <param name="textFontArguments">The font arguments of the text.</param>
        /// <param name="textMaterial">The material of the text.</param>
        public TextLabelDef(string text, float sizeX, float sizeY, FontArguments textFontArguments, Material textMaterial, 
            Color textColor, TextAlignment textHorizontalAlignment = TextAlignment.Left, TextAlignment textVerticalAlignment = TextAlignment.Center,
            TextAlignmentMode textAlignmentMode = TextAlignmentMode.Baseline, TextOverflowMode textOverflowMode = TextOverflowMode.Word)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            TextHorizontalAlignment = textHorizontalAlignment;
            TextVerticalAlignment = textVerticalAlignment;
            TextAlignmentMode = textAlignmentMode;
            TextOverflowMode = textOverflowMode;
            TextFontArguments = textFontArguments;
            TextMaterial = textMaterial;
            TextColor = textColor;
            Text = text;
        }
    }
}
