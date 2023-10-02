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
    public static class SpritesheetManager
    {
        public static IDictionary<Texture2D, SpritesheetElement> spritesheets = new Dictionary<Texture2D, SpritesheetElement>();

        public static void Add(Texture2D _texture, int _totalSpriteColumns, int _totalSpriteRows)
        {
            if (spritesheets.ContainsKey(_texture))
            {
                Console.WriteLine($"Trying to add a spritesheet that already exists.");
                return;
            }
            spritesheets.Add(_texture, new SpritesheetElement(_totalSpriteColumns, _totalSpriteRows));
        }

        public static Vector2 GetVertexUV(Texture2D _texture, int _col, int _row, Vector2 _localUV)
        {
            if(!spritesheets.ContainsKey(_texture))
            {
                Console.WriteLine("Requested a spritesheet that does not exist.");
                return Vector2.Zero;
            }

            if (_col >= spritesheets[_texture].totalSpriteColumns || _row >= spritesheets[_texture].totalSpriteRows)
            {
                Console.WriteLine("Requested a sprite that is out of the bounds of it's texture.");
                return Vector2.Zero;
            }

            // The UV stride of each sprite in the spritesheet
            Vector2 stride = new Vector2(1f / spritesheets[_texture].totalSpriteColumns, 1f / spritesheets[_texture].totalSpriteRows);

            return new Vector2((stride.X * _col) + (_localUV.X * stride.X), (stride.Y * _row) + (_localUV.Y * stride.Y));
        }
    }
}
