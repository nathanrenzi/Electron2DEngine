using System.Numerics;

namespace Electron2D
{
    public interface ITilemapRuleset
    {
        public Vector2 CheckRuleset(int[] _tileAndNeighbors3x3);
        public Vector2 CheckRuleset(int[] _tileAndNeighbors3x3, int[] _allowedTileTypes);
        public Vector2 CheckRulesetGetVertexUV(int[] _tileAndNeighbors3x3, Vector2 _localUV);
        public Vector2 CheckRulesetGetVertexUV(int[] _tileAndNeighbors3x3, int[] _allowedTileTypes, Vector2 _localUV);
    }
}
