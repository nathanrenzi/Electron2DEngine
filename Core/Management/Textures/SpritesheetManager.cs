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
        public static IDictionary<int, SpritesheetElement> spritesheets = new Dictionary<int, SpritesheetElement>();
        private static int spriteCursor;

        public static void Add(int _totalSpriteColumns, int _totalSpriteRows)
        {
            spritesheets.Add(spriteCursor, new SpritesheetElement(_totalSpriteColumns, _totalSpriteRows));
            spriteCursor++;
        }

        public static Vector2 GetVertexUV(int _spritesheetIndex, int _col, int _row, Vector2 _localUV)
        {
            if(!spritesheets.ContainsKey(_spritesheetIndex))
            {
                Console.WriteLine("Requested a spritesheet that does not exist.");
                return Vector2.Zero;
            }

            if (_col >= spritesheets[_spritesheetIndex].totalSpriteColumns || _row >= spritesheets[_spritesheetIndex].totalSpriteRows)
            {
                Console.WriteLine("Requested a sprite that is out of the bounds of it's texture.");
                return Vector2.Zero;
            }

            // The UV stride of each sprite in the spritesheet
            Vector2 stride = new Vector2(1f / spritesheets[_spritesheetIndex].totalSpriteColumns, 1f / spritesheets[_spritesheetIndex].totalSpriteRows);

            return new Vector2((stride.X * _col) + (_localUV.X * stride.X), (stride.Y * _row) + (_localUV.Y * stride.Y));
        }
    }
}
