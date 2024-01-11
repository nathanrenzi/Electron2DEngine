using static FreeTypeSharp.Native.FT;
using static Electron2D.OpenGL.GL;
using FreeTypeSharp;

namespace Electron2D.Core.Rendering.Text
{
    public class FontGlyphStore : IDisposable
    {
        private bool disposed;
        public bool Disposed => disposed;

        public uint TextureHandle { get; private set; }
        public int TextureAtlasWidth { get; private set; }
        public Dictionary<char, Character> Characters { get; } = new Dictionary<char, Character>();
        public FontArguments Arguments { get; }
        public FreeTypeLibrary Library { get; }
        public IntPtr Face { get; }
        public bool UseKerning { get; }

        public FontGlyphStore(uint _textureHandle, int _textureAtlasWidth, int _fontSize, string _fontFile, FreeTypeLibrary _library, IntPtr _face, bool _useKerning)
        {
            TextureHandle = _textureHandle;
            TextureAtlasWidth = _textureAtlasWidth;
            Library = _library;
            Face = _face;
            UseKerning = _useKerning;

            string[] split = _fontFile.Split("/");
            string fontName = split[split.Length - 1];
            Arguments = new FontArguments() { FontSize = _fontSize, FontName = fontName };
        }

        ~FontGlyphStore()
        {
            Dispose(false);
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
        private void Dispose(bool _safeToDisposeManagedObjects)
        {
            if(!disposed)
            {
                glDeleteTexture(TextureHandle);
                Characters.Clear();
                FT_Done_Face(Face);
                if (_safeToDisposeManagedObjects)
                {
                    Library.Dispose();
                }
                disposed = true;
            }
        }
    }
}