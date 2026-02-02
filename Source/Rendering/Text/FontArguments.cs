namespace Electron2D.Rendering.Text
{
    public struct FontArguments
    {
        public static readonly FontArguments Default = new FontArguments(ResourceManager.GetEngineResourcePath("Fonts/Roboto-Regular.ttf"), 12);

        public string FontFile;
        public int FontSize;
        public float FontScale = 1f;
        public int OutlineWidth = 0;

        public FontArguments(string fontName, int fontSize, float fontScale = 1f, int outlineWidth = 0)
        {
            FontFile = fontName;
            FontSize = fontSize;
            FontScale = fontScale;
            OutlineWidth = outlineWidth;
        }
    }
}