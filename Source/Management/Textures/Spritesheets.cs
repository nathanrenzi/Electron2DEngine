using Electron2D.Rendering;
using System.Numerics;

namespace Electron2D
{
    /// <summary>
    /// This class allows for spritesheet calculations and stores the data of all active spritesheets
    /// </summary>
    public static class Spritesheets
    {
        public static IDictionary<ITexture, SpritesheetElement> spritesheets = new Dictionary<ITexture, SpritesheetElement>();

        public static Vector2 GetVertexUVNonElement(int _totalSpriteColumns,
            int _totalSpriteRows, int _sampleColumn, int _sampleRow, Vector2 _localUV)
        {
            Vector2 stride = new Vector2(1f / _totalSpriteColumns, 1f / _totalSpriteRows);

            return new Vector2((stride.X * _sampleColumn) + (_localUV.X * stride.X),
                (stride.Y * _sampleRow) + (_localUV.Y * stride.Y));
        }

        public static void Add(ITexture _texture, int _totalSpriteColumns, int _totalSpriteRows)
        {
            if (spritesheets.ContainsKey(_texture))
            {
                Debug.LogWarning($"Trying to add a spritesheet that already exists.");
                return;
            }
            spritesheets.Add(_texture, new SpritesheetElement(_totalSpriteColumns, _totalSpriteRows));
        }

        public static Vector2 GetVertexUV(ITexture _texture, int _col, int _row, Vector2 _localUV)
        {
            if(!spritesheets.ContainsKey(_texture))
            {
                Debug.LogWarning("Requested a spritesheet that does not exist.");
                return _localUV;
            }

            if (_col >= spritesheets[_texture].totalSpriteColumns || _row >= spritesheets[_texture].totalSpriteRows)
            {
                Debug.LogWarning("Requested a sprite that is out of the bounds of it's texture.");
                return _localUV;
            }

            // The UV stride of each sprite in the spritesheet
            Vector2 stride = new Vector2(1f / spritesheets[_texture].totalSpriteColumns, 1f / spritesheets[_texture].totalSpriteRows);

            return new Vector2((stride.X * _col) + (_localUV.X * stride.X), (stride.Y * _row) + (_localUV.Y * stride.Y));
        }
    }
}
