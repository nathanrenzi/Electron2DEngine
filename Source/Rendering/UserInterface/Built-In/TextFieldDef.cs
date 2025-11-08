using Electron2D.Rendering;
using Electron2D.Rendering.Text;
using System.Drawing;
using System.Numerics;

namespace Electron2D.UserInterface
{
    public class TextFieldDef
    {
        public int CaretWidth;
        public int SizeX;
        public int SizeY;
        public Color TextColor;
        public Color PromptTextColor;
        public TextAlignment TextHorizontalAlignment;
        public TextAlignment TextVerticalAlignment;
        public TextAlignmentMode TextAlignmentMode;
        public TextOverflowMode TextOverflowMode;
        public Vector4 TextAreaPadding;
        public FontArguments TextFontArguments;
        public Material TextMaterial;
        public Material BackgroundPanelMaterial;
        public Material CaretMaterial;
        public SlicedPanelDef BackgroundPanelDef;
        public string Text;
        public string PromptText;
        public bool WaitForEnterKey;
        public int MaxCharacterCount;
        public int MaxLineCount;
        public bool UseHoverCursor;
        public GLFW.CursorType HoverCursorType;

        /// <summary>
        /// Creates a new definition for a text field.
        /// </summary>
        /// <param name="sizeX">The size of the field on the X axis.</param>
        /// <param name="sizeY">The size of the field on the Y axis.</param>
        /// <param name="textAreaPadding">The padding of the text area in relation to the background panel. The format is Left, Right, Top, Bottom.</param>
        /// <param name="textFontArguments">The font arguments of the text.</param>
        /// <param name="textMaterial">The material of the text.</param>
        /// <param name="backgroundPanelMaterial">The material of the background panel.</param>
        /// <param name="backgroundPanelDef">The definition of the background panel, if a sliced panel is desired. Otherwise, a default panel will be created.</param>
        /// <param name="startText">The starting text of the text field.</param>
        /// <param name="promptText">The prompt text displayed behind the user's text when the text field is empty.</param>
        /// <param name="waitForEnterKey">Should the <see cref="TextField.OnTextEntered"/> event be called when the enter key is pressed, or when the text is updated?</param>
        public TextFieldDef(int caretWidth, int sizeX, int sizeY, Vector4 textAreaPadding, FontArguments textFontArguments, Material textMaterial, 
            Color textColor, Color promptTextColor, Material backgroundPanelMaterial, string startText, string promptText, int maxCharacterCount,
            int maxLineCount = 1, TextAlignment textHorizontalAlignment = TextAlignment.Left, TextAlignment textVerticalAlignment = TextAlignment.Center,
            TextAlignmentMode textAlignmentMode = TextAlignmentMode.Baseline, TextOverflowMode textOverflowMode = TextOverflowMode.Word,
            SlicedPanelDef backgroundPanelDef = null, bool waitForEnterKey = false, bool useHoverCursor = true, GLFW.CursorType hoverCursor = GLFW.CursorType.Beam,
            Material caretMaterial = null)
        {
            CaretWidth = caretWidth;
            SizeX = sizeX;
            SizeY = sizeY;
            TextHorizontalAlignment = textHorizontalAlignment;
            TextVerticalAlignment = textVerticalAlignment;
            TextAlignmentMode = textAlignmentMode;
            TextOverflowMode = textOverflowMode;
            TextAreaPadding = textAreaPadding;
            TextFontArguments = textFontArguments;
            TextMaterial = textMaterial;
            TextColor = textColor;
            PromptTextColor = promptTextColor;
            BackgroundPanelMaterial = backgroundPanelMaterial;
            BackgroundPanelDef = backgroundPanelDef;
            Text = startText;
            PromptText = promptText;
            WaitForEnterKey = waitForEnterKey;
            MaxCharacterCount = maxCharacterCount;
            MaxLineCount = maxLineCount;
            CaretMaterial = caretMaterial;
            UseHoverCursor = useHoverCursor;
            HoverCursorType = hoverCursor;
        }
    }
}
