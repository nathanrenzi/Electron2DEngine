using System.Numerics;

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

        public static DefaultTilemapRuleset Instance { get; private set; } = new DefaultTilemapRuleset();

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

            List<(Neighbors[], int)> initialList = ruleset;
            int index = 0;
            if(U)
            {
                initialList = GetTilesWithNeighborType(initialList, Neighbors.Up);
                index = InterpretCompletionResults(CheckListForCompletion(initialList), index);
            }
            if (R)
            {
                initialList = GetTilesWithNeighborType(initialList, Neighbors.Right);
                index = InterpretCompletionResults(CheckListForCompletion(initialList), index);
            }
            if (D)
            {
                initialList = GetTilesWithNeighborType(initialList, Neighbors.Down);
                index = InterpretCompletionResults(CheckListForCompletion(initialList), index);
            }
            if (L)
            {
                initialList = GetTilesWithNeighborType(initialList, Neighbors.Left);
                index = InterpretCompletionResults(CheckListForCompletion(initialList), index);
            }
            if (TL)
            {
                initialList = GetTilesWithNeighborType(initialList, Neighbors.TopLeft);
                index = InterpretCompletionResults(CheckListForCompletion(initialList), index);
            }
            if (TR)
            {
                initialList = GetTilesWithNeighborType(initialList, Neighbors.TopRight);
                index = InterpretCompletionResults(CheckListForCompletion(initialList), index);
            }
            if (BL)
            {
                initialList = GetTilesWithNeighborType(initialList, Neighbors.BottomLeft);
                index = InterpretCompletionResults(CheckListForCompletion(initialList), index);
            }
            if (BR)
            {
                initialList = GetTilesWithNeighborType(initialList, Neighbors.BottomRight);
                index = InterpretCompletionResults(CheckListForCompletion(initialList), index);
            }

            return new Vector2()
        }

        private List<(Neighbors[], int)> GetTilesWithNeighborType(List<(Neighbors[], int)> list, Neighbors neighbor)
        {
            if (list.Count == 0) return list;

            List<(Neighbors[], int)> returnValues = new List<(Neighbors[], int)> ();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Item1 == null) continue; // Skipping empty tile
                for (int x = 0; x < list[i].Item1.Length; x++)
                {
                    if (list[i].Item1[x] == neighbor)
                    {
                        returnValues.Add(list[i]);
                        continue;
                    }
                }
            }

            return returnValues;
        }

        private int InterpretCompletionResults(int result, int currentIndex)
        {
            if(result == -1)
            {
                return currentIndex;
            }
            else
            {
                return result;
            }
        }

        private int CheckListForCompletion(List<(Neighbors[], int)> list)
        {
            if (list.Count == 0)
            {
                return 0; // Returns the first index of the ruleset, which is a tile with no neighbors
            }
            else if(list.Count == 1)
            {
                return list[0].Item2;
            }
            else
            {
                return -1;
            }
        }
    }
}
