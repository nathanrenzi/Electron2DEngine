using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Electron2D.Core.Management
{
    /// <summary>
    /// This struct represents a spritesheet on one of the texture slots in the GPU
    /// </summary>
    public struct SpritesheetElement
    {
        public readonly int totalSpriteColumns;
        public readonly int totalSpriteRows;

        // Eventually add auto generated spritesheets
        public SpritesheetElement(int _totalSpriteColumns, int _totalSpriteRows)
        {
            totalSpriteColumns = _totalSpriteColumns;
            totalSpriteRows = _totalSpriteRows;
        }
    }
}
