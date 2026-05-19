using static FreeTypeSharp.Native.FT;
using static Atlas2D.OpenGL.GL;
using FreeTypeSharp;

namespace Atlas2D.Rendering.Text
{
    public class FontGlyphStore : IDisposable
    {
        public SharedResource<Texture2D> Texture { get; }
        public Dictionary<char, Character> Characters { get; } = new Dictionary<char, Character>();
        public FontArgs Arguments { get; }
        public FreeTypeLibrary Library { get; }
        public IntPtr Face { get; }
        public bool UseKerning { get; }
        public int Ascent { get; private set; }
        public int Descent { get; private set; }
        private bool _isDone = false;

        public FontGlyphStore(SharedResource<Texture2D> texture, int fontSize, string fontFile, FreeTypeLibrary library, IntPtr face, bool useKerning)
        {
            Texture = texture.AddRef();
            Library = library;
            Face = face;
            UseKerning = useKerning;

            Arguments = new FontArgs() { FontSize = fontSize, FontFile = fontFile };
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
            Characters.Clear();
            FT_Done_Face(Face);
            Texture.Release();
            Library.Dispose();
        }
    }
}