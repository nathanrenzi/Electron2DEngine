using static FreeTypeSharp.Native.FT;
using static Electron2D.OpenGL.GL;
using FreeTypeSharp;

namespace Electron2D.Rendering.Text
{
    public class FontGlyphStore : IDisposable
    {
        private bool _disposed;
        public bool Disposed => _disposed;

        public uint TextureHandle { get; private set; }
        public int TextureAtlasWidth { get; private set; }
        public int TextureAtlasHeight { get; private set; }
        public Dictionary<char, Character> Characters { get; } = new Dictionary<char, Character>();
        public FontArguments Arguments { get; }
        public FreeTypeLibrary Library { get; }
        public IntPtr Face { get; }
        public bool UseKerning { get; }
        public int Ascent { get; private set; }
        public int Descent { get; private set; }
        private bool _isDone = false;

        public FontGlyphStore(uint textureHandle, int textureAtlasWidth, int textureAtlasHeight, int fontSize, string fontFile, FreeTypeLibrary library, IntPtr face, bool useKerning)
        {
            TextureHandle = textureHandle;
            TextureAtlasWidth = textureAtlasWidth;
            TextureAtlasHeight = textureAtlasHeight;
            Library = library;
            Face = face;
            UseKerning = useKerning;

            Arguments = new FontArguments() { FontSize = fontSize, FontFile = fontFile };
        }

        ~FontGlyphStore()
        {
            Dispose(false);
        }

        public void AddCharacter(char code, Character character)
        {
            if(_isDone) return;
            Characters.Add(code, character);
        }

        public void Done()
        {
            if (_isDone) return;
            _isDone = true;
            foreach (var character in Characters.Values)
            {
                int descent = (int)(character.Size.Y - character.Bearing.Y);
                int ascent = (int)(character.Size.Y - descent);
                if(ascent > Ascent)
                {
                    Ascent = ascent;
                }
                if(descent > Descent)
                {
                    Descent = descent;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool safeToDisposeManagedObjects)
        {
            if(!_disposed)
            {
                glDeleteTexture(TextureHandle);
                Characters.Clear();
                FT_Done_Face(Face);
                if (safeToDisposeManagedObjects)
                {
                    Library.Dispose();
                }
                _disposed = true;
            }
        }
    }
}