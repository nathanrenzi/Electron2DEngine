using Electron2D.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Electron2D.Core.Management.Textures
{
    /// <summary>
    /// This class allows for spritesheet calculations and stores the data of all active spritesheets
    /// </summary>
    public static class Spritesheets
    {
        public static IDictionary<ITexture, SpritesheetElement> spritesheets = new Dictionary<ITexture, SpritesheetElement>();

        public static void Add(ITexture _texture, int _totalSpriteColumns, int _totalSpriteRows)
        {
            if (spritesheets.ContainsKey(_texture))
            {
                Console.WriteLine($"Trying to add a spritesheet that already exists.");
                return;
            }
            spritesheets.Add(_texture, new SpritesheetElement(_totalSpriteColumns, _totalSpriteRows));
        }

        public static Vector2 GetVertexUV(ITexture _texture, int _col, int _row, Vector2 _localUV)
        {
            if(!spritesheets.ContainsKey(_texture))
            {
                Console.WriteLine("Requested a spritesheet that does not exist.");
                return _localUV;
            }

            if (_col >= spritesheets[_texture].totalSpriteColumns || _row >= spritesheets[_texture].totalSpriteRows)
            {
                Console.WriteLine("Requested a sprite that is out of the bounds of it's texture.");
                return _localUV;
            }

            // The UV stride of each sprite in the spritesheet
            Vector2 stride = new Vector2(1f / spritesheets[_texture].totalSpriteColumns, 1f / spritesheets[_texture].totalSpriteRows);

            return new Vector2((stride.X * _col) + (_localUV.X * stride.X), (stride.Y * _row) + (_localUV.Y * stride.Y));
        }
    }
}
