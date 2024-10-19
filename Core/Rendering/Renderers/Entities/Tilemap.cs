using Electron2D.Core.ECS;
using Electron2D.Core.Management.Textures;
using Electron2D.Core.Rendering;
using Newtonsoft.Json;
using System.Numerics;
using System.Text;

namespace Electron2D.Core
{
    public struct TileData
    {
        public string Name { get; set; } = "";
        public int SpriteColumn { get; set; } = 0;
        public int SpriteRow { get; set; } = 0;
        public Material Material { get; set; } = null;

        public TileData(string _name, Material _material, int _spriteColumn = 0, int _spriteRow = 0)
        {
            Name = _name;
            Material = _material;
            SpriteColumn = _spriteColumn;
            SpriteRow = _spriteRow;
        }
    }

    public struct TileMesh
    {
        public List<float> Vertices;
        public List<uint> Indices;
        public MeshRenderer Renderer;

        public TileMesh(Transform _transform, Material _material)
        {
            Vertices = new List<float>();
            Indices = new List<uint>();
            Renderer = new MeshRenderer(_transform, _material);
        }
    }

    public class Tilemap : Entity, IRenderable
    {
        public TileData[] Data { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }
        public int[] Tiles { get; set; }
        public int TilePixelSize { get; set; }
        public int RenderLayer;

        private int realTilePixelSize
        {
            get
            {
                return TilePixelSize * 2; // Compensating for Transform 0.5x scaling
            }
        }
        private Dictionary<Material, TileMesh> meshDataDictionary = new Dictionary<Material, TileMesh>();
        private Transform transform;
        private bool isDirty = false;

        public Tilemap(TileData[] _data, int[] _tileArray, int _tilePixelSize, int _sizeX, int _sizeY, int _renderLayer = -1)
        {
            TilePixelSize = _tilePixelSize;
            Data = _data;
            Tiles = _tileArray;
            SizeX = _sizeX;
            SizeY = _sizeY;
            RenderLayer = _renderLayer;

            transform = new Transform();
            AddComponent(transform);

            // Add renderer for each new material
            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i].Material == null) continue;
                if (!meshDataDictionary.ContainsKey(Data[i].Material))
                {
                    meshDataDictionary.Add(Data[i].Material, new TileMesh(transform, Data[i].Material));
                }
            }

            isDirty = true;

            Game.OnUpdateEvent += RegenerateEntireMesh;
            RenderLayerManager.OrderRenderable(this);
        }

        ~Tilemap()
        {
            Game.OnUpdateEvent -= RegenerateEntireMesh;
            RenderLayerManager.RemoveRenderable(this);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static Tilemap FromJson(string _filePath, Material _material) // Temporary material assignment, will be done through JSON / specific material loading from disk system
        {
            return null;
        }

        private void RegenerateEntireMesh()
        {
            if (!isDirty) return;
            isDirty = false;

            for (int i = 0; i < Tiles.Length; i++)
            {
                if (Tiles[i] == -1) continue; // If the tile is empty (-1), skip
                TileData data = Data[Tiles[i]];
                TileMesh mesh = meshDataDictionary[data.Material];

                float xPos = (i % SizeX) * realTilePixelSize;
                float yPos = (i / SizeX) * realTilePixelSize;

                for (int a = 0; a < 4; a++) // Each vertex in the quad
                {
                    float xMod = 0;
                    float yMod = 0;
                    switch (a)
                    {
                        case 0:
                            xMod = realTilePixelSize;
                            yMod = realTilePixelSize;
                            break;
                        case 1:
                            xMod = 0;
                            yMod = realTilePixelSize;
                            break;
                        case 2:
                            xMod = 0;
                            yMod = 0;
                            break;
                        case 3:
                            xMod = realTilePixelSize;
                            yMod = 0;
                            break;
                    }

                    // Vertices
                    mesh.Vertices.Add(xPos + xMod); // X
                    mesh.Vertices.Add(yPos + yMod); // Y

                    // UV
                    float u = xMod / realTilePixelSize;
                    float v = yMod / realTilePixelSize;
                    Vector2 newUV = Spritesheets.spritesheets.ContainsKey(data.Material.MainTexture) ?
                        Spritesheets.GetVertexUV(data.Material.MainTexture,
                        data.SpriteColumn, data.SpriteRow, new Vector2(u, v)) :
                        new Vector2(u, v);
                    mesh.Vertices.Add(newUV.X); // U
                    mesh.Vertices.Add(newUV.Y); // V
                }

                // Indices
                int vertices = (mesh.Vertices.Count / 4) - 4;
                mesh.Indices.Add((uint)(vertices + 0));
                mesh.Indices.Add((uint)(vertices + 1));
                mesh.Indices.Add((uint)(vertices + 2));
                mesh.Indices.Add((uint)(vertices + 0));
                mesh.Indices.Add((uint)(vertices + 2));
                mesh.Indices.Add((uint)(vertices + 3));
            }

            foreach (var m in meshDataDictionary)
            {
                if(m.Value.Vertices.Count > 0)
                {
                    m.Value.Renderer.SetVertexArrays(m.Value.Vertices.ToArray(), m.Value.Indices.ToArray());
                }
            }
        }

        public void SetTileID(int _x, int _y, byte _tileID) { Tiles[_x + _y * SizeY] = _tileID; isDirty = true; }
        public int GetTileID(int _x, int _y) => Tiles[_x + _y * SizeY];
        public TileData GetTileData(int _x, int _y) => Data[GetTileID(_x, _y)];

        public int GetRenderLayer() => RenderLayer;

        public void Render()
        {
            foreach (var m in meshDataDictionary)
            {
                m.Value.Renderer.Render();
            }
        }
    }
}
