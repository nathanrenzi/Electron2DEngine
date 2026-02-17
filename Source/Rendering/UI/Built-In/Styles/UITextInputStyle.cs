using System.Drawing;

namespace Electron2D.UI
{
    public sealed class UITextInputStyle
    {
        public UITextStyle TextStyle { get; }
        public UIPanelDef BackgroundDef { get; }
        public UIPanelDef? CaretDef { get; }
        public Color TextColor { get; }
        public Color PromptTextColor { get; }
        public int CaretWidth { get; }
        public Border TextAreaPadding { get; }


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
