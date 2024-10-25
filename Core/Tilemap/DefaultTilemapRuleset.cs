using System;
using System.Numerics;
using System.Text;

namespace Electron2D.Core
{
    public class DefaultTilemapRuleset : ITilemapRuleset
    {
        private enum Neighbors
        {
            None,
            Up,
            Right,
            Down,
            Left,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        // Read from bottom left to the right, and then going up a level when at the end
        private static List<(Neighbors[], int)> ruleset { get; } = new List<(Neighbors[], int)>()
        {
            (new Neighbors[] { Neighbors.None }, 0),
            (new Neighbors[] { Neighbors.Right }, 1),
            (new Neighbors[] { Neighbors.Right, Neighbors.Left }, 2),
            (new Neighbors[] { Neighbors.Left }, 3),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Right, Neighbors.Left, Neighbors.BottomLeft }, 4),
            (new Neighbors[] { Neighbors.Up, Neighbors.Right, Neighbors.Left, Neighbors.TopRight }, 5),
            (new Neighbors[] { Neighbors.Up, Neighbors.Right, Neighbors.Left, Neighbors.TopLeft }, 6),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Right, Neighbors.Left, Neighbors.BottomRight }, 7),
            (new Neighbors[] { Neighbors.Up, Neighbors.Right, Neighbors.TopRight }, 8),
            (new Neighbors[] { Neighbors.Up, Neighbors.Right, Neighbors.Left, Neighbors.TopRight, Neighbors.TopLeft }, 9),
            (new Neighbors[] { Neighbors.Up, Neighbors.Right, Neighbors.Down, Neighbors.Left, Neighbors.TopRight, Neighbors.TopLeft }, 10),
            (new Neighbors[] { Neighbors.Up, Neighbors.Left, Neighbors.TopLeft }, 11),
            (new Neighbors[] { Neighbors.Up }, 12),
            (new Neighbors[] { Neighbors.Up, Neighbors.Right }, 13),
            (new Neighbors[] { Neighbors.Up, Neighbors.Right, Neighbors.Left }, 14),
            (new Neighbors[] { Neighbors.Up, Neighbors.Left }, 15),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Right, Neighbors.TopRight }, 16),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.TopLeft, Neighbors.TopRight, Neighbors.BottomRight }, 17),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.TopLeft, Neighbors.TopRight, Neighbors.BottomLeft }, 18),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.TopLeft }, 19),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.TopRight, Neighbors.BottomRight }, 20),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.TopLeft, Neighbors.TopRight, Neighbors.BottomLeft, Neighbors.BottomRight }, 21),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.TopLeft, Neighbors.BottomRight }, 22),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.TopLeft, Neighbors.BottomLeft }, 23),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down }, 24),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Right }, 25),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right }, 26),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left }, 27),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Right, Neighbors.BottomRight }, 28),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.TopRight, Neighbors.BottomLeft, Neighbors.BottomRight }, 29),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.TopLeft, Neighbors.BottomLeft, Neighbors.BottomRight }, 30),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.BottomLeft }, 31),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Right, Neighbors.TopRight, Neighbors.BottomRight }, 32),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.TopRight, Neighbors.BottomLeft }, 33),
            (null, 34), // Empty tile, just taking up space so that index is correct
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.TopLeft, Neighbors.BottomLeft }, 35),
            (new Neighbors[] { Neighbors.Down }, 36),
            (new Neighbors[] { Neighbors.Down, Neighbors.Right }, 37),
            (new Neighbors[] { Neighbors.Down, Neighbors.Left, Neighbors.Right }, 38),
            (new Neighbors[] { Neighbors.Down, Neighbors.Left }, 39),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.TopLeft }, 40),
            (new Neighbors[] { Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.BottomRight }, 41),
            (new Neighbors[] { Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.BottomLeft }, 42),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.TopRight }, 43),
            (new Neighbors[] { Neighbors.Down, Neighbors.Right, Neighbors.BottomRight }, 44),
            (new Neighbors[] { Neighbors.Up, Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.BottomLeft, Neighbors.BottomRight }, 45),
            (new Neighbors[] { Neighbors.Down, Neighbors.Left, Neighbors.Right, Neighbors.BottomLeft, Neighbors.BottomRight }, 46),
            (new Neighbors[] { Neighbors.Down, Neighbors.Left, Neighbors.BottomLeft }, 47),
        };

        public Vector2 CheckRuleset(int[] _tileAndNeighbors3x3)
        {
            int tileType = _tileAndNeighbors3x3[4]; // Middle tile of 3x3 grid
            return CheckRuleset(_tileAndNeighbors3x3, new int[] { tileType });
        }

        public Vector2 CheckRuleset(int[] _tileAndNeighbors3x3, int[] _allowedTileTypes)
        {
            if(_tileAndNeighbors3x3.Length != 9)
            {
                Debug.LogError("Tile and neighbors array passed to ruleset does not have 9 elements," +
                    $"has {_tileAndNeighbors3x3.Length} elements");
                return Vector2.Zero;
            }

            bool U, R, D, L, TL, TR, BL, BR;
            U = _allowedTileTypes.Contains(_tileAndNeighbors3x3[7]);
            R = _allowedTileTypes.Contains(_tileAndNeighbors3x3[5]);
            D = _allowedTileTypes.Contains(_tileAndNeighbors3x3[1]);
            L = _allowedTileTypes.Contains(_tileAndNeighbors3x3[3]);
            TL = _allowedTileTypes.Contains(_tileAndNeighbors3x3[6]);
            TR = _allowedTileTypes.Contains(_tileAndNeighbors3x3[8]);
            BL = _allowedTileTypes.Contains(_tileAndNeighbors3x3[0]);
            BR = _allowedTileTypes.Contains(_tileAndNeighbors3x3[2]);

            List<Neighbors> neighborTypes = new List<Neighbors>();
            if (U) neighborTypes.Add(Neighbors.Up);
            if (R) neighborTypes.Add(Neighbors.Right);
            if (D) neighborTypes.Add(Neighbors.Down);
            if (L) neighborTypes.Add(Neighbors.Left);
            if (TL) neighborTypes.Add(Neighbors.TopLeft);
            if (TR) neighborTypes.Add(Neighbors.TopRight);
            if (BL) neighborTypes.Add(Neighbors.BottomLeft);
            if (BR) neighborTypes.Add(Neighbors.BottomRight);

            List<(Neighbors[], int)> matches = ruleset;
            int index = CheckTilesForMatch(matches, neighborTypes);
            Debug.EnableCollapsing = false;
            Debug.Log(index);
            Debug.EnableCollapsing = true;

            int y = index / 12; // Ruleset template is 12 tiles wide
            int x = index - (y * 12);

            return new Vector2(x, y);
        }

        private int CheckTilesForMatch(List<(Neighbors[], int)> possibleMatches, List<Neighbors> neighbors)
        {
            int savedIndex = 0;

            for (int i = 0; i < possibleMatches.Count; i++)
            {
                if (possibleMatches[i].Item1 == null) continue;
                if (CheckNotMatch(possibleMatches[i], neighbors, Neighbors.Up)) continue;
                if (CheckNotMatch(possibleMatches[i], neighbors, Neighbors.Right)) continue;
                if (CheckNotMatch(possibleMatches[i], neighbors, Neighbors.Down)) continue;
                if (CheckNotMatch(possibleMatches[i], neighbors, Neighbors.Left)) continue;
                savedIndex = possibleMatches[i].Item2; // Saving the index in case the corner checks fail
                if(neighbors.Contains(Neighbors.Up))
                {
                    if(neighbors.Contains(Neighbors.Left))
                    {
                        if (CheckNotMatch(possibleMatches[i], neighbors, Neighbors.TopLeft)) continue;
                    }
                    if(neighbors.Contains(Neighbors.Right))
                    {
                        if (CheckNotMatch(possibleMatches[i], neighbors, Neighbors.TopRight)) continue;
                    }
                }
                if(neighbors.Contains(Neighbors.Down))
                {
                    if(neighbors.Contains(Neighbors.Left))
                    {
                        if (CheckNotMatch(possibleMatches[i], neighbors, Neighbors.BottomLeft)) continue;
                    }
                    if(neighbors.Contains(Neighbors.Right))
                    {
                        if (CheckNotMatch(possibleMatches[i], neighbors, Neighbors.BottomRight)) continue;
                    }
                }

                return possibleMatches[i].Item2;
            }

            return savedIndex;
        }

        private bool CheckNotMatch((Neighbors[], int) match, List<Neighbors> neighbors, Neighbors neighborToCheck)
        {
            bool fail = false;
            fail = !match.Item1.Contains(neighborToCheck) && neighbors.Contains(neighborToCheck);
            fail = fail || match.Item1.Contains(neighborToCheck) && !neighbors.Contains(neighborToCheck);
            return fail;
        }

        public Vector2 CheckRulesetGetVertexUV(int[] _tileAndNeighbors3x3, Vector2 _localUV)
        {
            int tileType = _tileAndNeighbors3x3[4];
            return CheckRulesetGetVertexUV(_tileAndNeighbors3x3, new int[] { tileType }, _localUV);
        }

        public Vector2 CheckRulesetGetVertexUV(int[] _tileAndNeighbors3x3, int[] _allowedTileTypes, Vector2 _localUV)
        {
            Vector2 pos = CheckRuleset(_tileAndNeighbors3x3, _allowedTileTypes);
            return Spritesheets.GetVertexUVNonElement(12, 4, (int)pos.X, (int)pos.Y, _localUV);
        }
    }
}
