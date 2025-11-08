namespace Electron2D.Rendering.Text
{
    public struct FontArguments
    {
        public string FontFile;
        public int FontSize;
        public int OutlineWidth;

        public FontArguments(string fontName, int fontSize, int outlineWidth)
        {
            FontFile = fontName;
            FontSize = fontSize;
            OutlineWidth = outlineWidth;
        }
    }
}