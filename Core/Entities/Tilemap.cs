using Electron2D.Core.ECS;
using Electron2D.Core.Management.Textures;
using Electron2D.Core.Rendering;
using System.Numerics;

namespace Electron2D.Core
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

        public TileData[] TileData { get; private set; }
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
            TileData = _tileData;
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

                float xPos = (i % sizeX) * tilePixelSize;
                float yPos = (i / sizeX) * tilePixelSize;

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
                    vertices[i * stride * 4 + a * stride + 0] = xPos + xMod;            // X
                    vertices[i * stride * 4 + a * stride + 1] = yPos + yMod;            // Y

                    // Add submesh sprite support here using the 0, 1 values generated
                    float u = xMod / tilePixelSize;
                    float v = yMod / tilePixelSize;
                    TileData data = TileData[tiles[i]];
                    Vector2 newUV = SpritesheetManager.spritesheets.ContainsKey(renderer.Material.MainTexture) ? 
                        SpritesheetManager.GetVertexUV(renderer.Material.MainTexture,
                        data.SpriteColumn, data.SpriteRow, new Vector2(u, v)) : 
                        new Vector2(u, v);

                    vertices[i * stride * 4 + a * stride + 2] = newUV.X;                // U
                    vertices[i * stride * 4 + a * stride + 3] = newUV.Y;                // V
                }

                // Indices
                int index = realTileCount * 6; // Multiplying by 6 because there are 6 indices per quad
                int offset = realTileCount * 4; // Multiplying by 4 because there are 4 vertices per quad
                indices[index + 0] = (uint)(offset + 0);
                indices[index + 1] = (uint)(offset + 1);
                indices[index + 2] = (uint)(offset + 2);
                indices[index + 3] = (uint)(offset + 0);
                indices[index + 4] = (uint)(offset + 2);
                indices[index + 5] = (uint)(offset + 3);

                realTileCount++;
            }

            renderer.SetVertexArrays(vertices, indices);
        }

        public void SetTileID(int _x, int _y, byte _tileID) { tiles[_x + _y * sizeY] = _tileID; isDirty = true; }
        public byte GetTileID(int _x, int _y) => tiles[_x + _y * sizeY];
        public TileData GetTileData(int _x, int _y) => TileData[GetTileID(_x, _y)];
    }
}
