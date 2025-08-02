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
        public FontGlyphStore TextFont;
        public Material TextMaterial;
        public string Text;

        /// <summary>
        /// Creates a new definition for a text label.
        /// </summary>
        /// <param name="sizeX">The size of the field on the X axis.</param>
        /// <param name="sizeY">The size of the field on the Y axis.</param>
        /// <param name="textAreaPadding">The padding of the text area in relation to the background panel. The format is Left, Right, Top, Bottom.</param>
        /// <param name="textFont">The font of the text.</param>
        /// <param name="textMaterial">The material of the text.</param>
        /// <param name="backgroundPanelMaterial">The material of the background panel.</param>
        /// <param name="backgroundPanelDef">The definition of the background panel, if a sliced panel is desired. Otherwise, a default panel will be created.</param>
        /// <param name="startText">The starting text of the text field.</param>
        /// <param name="promptText">The prompt text displayed behind the user's text when the text field is empty.</param>
        /// <param name="waitForEnterKey">Should the <see cref="TextField.OnTextEntered"/> event be called when the enter key is pressed, or when the text is updated?</param>
        public TextLabelDef(string text, float sizeX, float sizeY, FontGlyphStore textFont, Material textMaterial, 
            Color textColor, TextAlignment textHorizontalAlignment = TextAlignment.Left, TextAlignment textVerticalAlignment = TextAlignment.Center,
            TextAlignmentMode textAlignmentMode = TextAlignmentMode.Baseline, TextOverflowMode textOverflowMode = TextOverflowMode.Word)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            TextHorizontalAlignment = textHorizontalAlignment;
            TextVerticalAlignment = textVerticalAlignment;
            TextAlignmentMode = textAlignmentMode;
            TextOverflowMode = textOverflowMode;
            TextFont = textFont;
            TextMaterial = textMaterial;
            TextColor = textColor;
            Text = text;
        }
    }
}
