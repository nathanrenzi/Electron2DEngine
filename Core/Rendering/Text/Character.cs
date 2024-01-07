using System.Numerics;

namespace Electron2D.Core.Rendering.Text
{
    public struct Character
    {
        public Vector2 Size { get; }        // Size of glyph in game scene
        public Vector2 UVX { get; }         // X-Axis UV (Left, Right)
        public Vector2 UVY { get; }         // Y-Axis UV (Top, Bottom)
        public Vector2 Bearing { get; }     // Offset from baseline to left/top of glyph
        public uint Advance { get; }        // Offset to advance to next glyph

        public Character(Vector2 _size, Vector2 _uvx, Vector2 _uvy, Vector2 _bearing, uint _advance)
        {
            Size = _size;
            UVX = _uvx;
            UVY = _uvy;
            Bearing = _bearing;
            Advance = _advance;
        }
    }
}
