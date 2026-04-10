using System.Drawing;

namespace Electron2D.UI
{
    /// <summary>
    /// Defines the visual style of a <see cref="UITextInput"/>.
    /// </summary>
    public sealed class UITextInputStyle
    {
        /// <summary>
        /// The text style used for the text.
        /// </summary>
        public UITextStyle TextStyle { get; }

        /// <summary>
        /// The panel definition for the background.
        /// </summary>
        public UIPanelDef BackgroundDef { get; }

        /// <summary>
        /// A custom panel definition for the caret, or <see langword="null"/> to use the default.
        /// </summary>
        public UIPanelDef? CaretDef { get; }

        /// <summary>
        /// The color of the input text.
        /// </summary>
        public Color TextColor { get; }

        /// <summary>
        /// The color of the prompt text shown when the input is empty.
        /// </summary>
        public Color PromptTextColor { get; }

        /// <summary>
        /// The width of the caret in pixels.
        /// </summary>
        public int CaretWidth { get; }

        /// <summary>
        /// The padding around the text area.
        /// </summary>
        public Border TextAreaPadding { get; }

        /// <summary>
        /// Creates a new <see cref="UITextInputStyle"/>.
        /// </summary>
        /// <param name="textStyle">The text style used for the text.</param>
        /// <param name="backgroundDef">The panel definition for the background.</param>
        /// <param name="textAreaPadding">The padding around the text area. Defaults to zero.</param>
        /// <param name="promptTextColor">The color of the prompt text. Defaults to white.</param>
        /// <param name="caretDef">A custom panel definition for the caret. Defaults to <see langword="null"/>.</param>
        /// <param name="caretWidth">The width of the caret in pixels. Defaults to 1.</param>
        public UITextInputStyle(UITextStyle textStyle, UIPanelDef backgroundDef, Border? textAreaPadding = null,
            Color? promptTextColor = null, UIPanelDef? caretDef = null, int caretWidth = 1)
        {
            TextStyle = textStyle;
            BackgroundDef = backgroundDef;
            TextColor = textStyle.Color;
            TextAreaPadding = textAreaPadding ?? new Border(0);
            PromptTextColor = promptTextColor ?? Color.White;
            CaretDef = caretDef;
            CaretWidth = caretWidth;
        }
    }
}