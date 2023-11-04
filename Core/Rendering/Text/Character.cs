using System.Numerics;

namespace Electron2D.Core.Rendering.Text
{
    public struct Character
    {
        public uint TextureHandle { get; }  // Handle of the texture object
        public Vector2 Size { get; }        // Size of glyph
        public Vector2 Bearing { get; }     // Offset from baseline to left/top of glyph
        public uint Advance { get; }        // Offset to advance to next glyph

        public Character(uint _textureHandle, Vector2 _size, Vector2 _bearing, uint _advance)
        {
            TextureHandle = _textureHandle;
            Size = _size;
            Bearing = _bearing;
            Advance = _advance;
        }
    }
}
