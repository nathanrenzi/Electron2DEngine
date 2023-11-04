using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;
using static Electron2D.OpenGL.GL;
using FreeTypeSharp;
using GLFW;

namespace Electron2D.Core.Rendering.Text
{
    public class FontGlyphStore : IDisposable
    {
        private bool disposed;
        public bool Disposed => disposed;

        public Dictionary<char, Character> Characters { get; } = new Dictionary<char, Character>();
        public int FontSize;
        public string FontFile;

        public FontGlyphStore(int _fontSize, string _fontFile)
        {
            FontSize = _fontSize;
            FontFile = _fontFile;
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