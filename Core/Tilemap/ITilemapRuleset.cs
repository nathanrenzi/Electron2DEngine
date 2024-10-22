using System.Numerics;

namespace Electron2D.Core
{
    public interface ITilemapRuleset
    {
        public Vector2 CheckRuleset(int[] _tileAndNeigbors3x3);
        public Vector2 CheckRuleset(int[] _tileAndNeighbors3x3, int[] _allowedTileTypes);
    }
}
