using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering.Text
{
    public class FontGlyphStore : IDisposable
    {
        private bool disposed;
        public bool Disposed => disposed;

        public Dictionary<char, Character> Characters { get; } = new Dictionary<char, Character>();
        public FontArguments Arguments { get; }

        public FontGlyphStore(int _fontSize, string _fontFile)
        {
            string[] split = _fontFile.Split("/");
            string fontName = split[split.Length - 1];
            Arguments = new FontArguments() { FontSize = _fontSize, FontName = fontName };
        }

        public void AddCharacter(char _char, Character _character)
        {
            Characters.Add(_char, _character);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool _disposing)
        {
            if (Characters.Count != 0)
            {
                foreach (var item in Characters)
                {
                    glDeleteTexture(item.Value.TextureHandle);
                }
                Characters.Clear();
            }

            disposed = true;
        }
    }
}