using Electron2D.Core.ECS;
using Electron2D.Core.Rendering;

namespace Electron2D.Core.Mapping
{
    public struct TileData
    {
        public string Name { get; private set; }
        public int SpriteColumn { get; private set; }
        public int SpriteRow { get; private set; }

        public TileData(string _name, int _spriteColumn, int _spriteRow)
        {
            Name = _name;
            SpriteColumn = _spriteColumn;
            SpriteRow = _spriteRow;
        }
    }

    public class Tilemap : Entity
    {
        // Add chunking - will need a TileChunk class with it's own meshrenderer

        public TileData[] tileData { get; private set; }
        private int sizeX;
        private int sizeY;
        private byte[] tiles;
        private int tilePixelSize;

        private Transform transform;
        private MeshRenderer renderer;

        private bool isDirty = false;

        public Tilemap(Material _material, TileData[] _tileData, int _tilePixelSize, int _sizeX, int _sizeY, byte[] _tileArray, int _renderLayer = -1)
        {
            tilePixelSize = _tilePixelSize;
            tileData = _tileData;
            sizeX = _sizeX;
            sizeY = _sizeY;
            tiles = _tileArray;

            transform = new Transform();
            AddComponent(transform);

            renderer = new MeshRenderer(transform, _material, _renderLayer);
            AddComponent(renderer);

            isDirty = true;

            Game.OnUpdateEvent += RegenerateMesh;
        }

        private void RegenerateMesh()
        {
            if (!isDirty) return;
            isDirty = false;

            int stride = 4;
            float[] vertices = new float[tiles.Length * 4 * stride]; // 4 vertices per tile, x, y, u, v, cut out temp: r, g, b, a
            uint[] indices = new uint[tiles.Length * 6];             // 6 indices per quad
            int realTileCount = 0;

            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] == 255) continue; // If the tile is empty (255), skip

                float xPos = (i % stride) * tilePixelSize;
                float yPos = (i / stride) * tilePixelSize;

                for (int a = 0; a < 4; a++) // Each vertex in the quad
                {
                    float xMod = 0;
                    float yMod = 0;
                    switch (a)
                    {
                        case 0:
                            xMod = tilePixelSize;
                            yMod = tilePixelSize;
                            break;
                        case 1:
                            xMod = 0;
                            yMod = tilePixelSize;
                            break;
                        case 2:
                            xMod = 0;
                            yMod = 0;
                            break;
                        case 3:
                            xMod = tilePixelSize;
                            yMod = 0;
                            break;
                    }

                    // Vertices
                    vertices[(i * stride * 4) + (a * stride) + 0] = xPos + xMod;
                    vertices[(i * stride * 4) + (a * stride) + 1] = yPos + yMod;
                    vertices[(i * stride * 4) + (a * stride) + 2] = xMod / tilePixelSize;
                    vertices[(i * stride * 4) + (a * stride) + 2] = yMod / tilePixelSize;
                }

                // Indices
                int index = realTileCount * 6;
                indices[index + 0] = (uint)(realTileCount + 0);
                indices[index + 1] = (uint)(realTileCount + 1);
                indices[index + 2] = (uint)(realTileCount + 2);
                indices[index + 3] = (uint)(realTileCount + 0);
                indices[index + 4] = (uint)(realTileCount + 2);
                indices[index + 5] = (uint)(realTileCount + 3);

                realTileCount++;
            }

            renderer.SetVertexArrays(vertices, indices);
        }

        public void SetTileID(int _x, int _y, byte _tileID) { tiles[_x + (_y * sizeY)] = _tileID; isDirty = true; }
        public byte GetTileID(int _x, int _y) => tiles[_x + (_y * sizeY)];
        public TileData GetTileData(int _x, int _y) => tileData[GetTileID(_x, _y)];
    }
}
