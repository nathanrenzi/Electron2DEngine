﻿using Electron2D.Core.ECS;
using Electron2D.Core.PhysicsBox2D;
using Electron2D.Core.Rendering;
using Newtonsoft.Json;
using System.Numerics;
using System.Text;

namespace Electron2D.Core
{
    public class Tilemap : Entity, IRenderable
    {
        public TileData[] Data { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }
        public int[] Tiles { get; set; }
        public byte[] TileRotations { get; set; }
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
        private Random random;
        private int seed;
        private bool isDirty = false;

        private Tilemap(TileData[] _data, int[] _tileArray, int _tilePixelSize,
            int _sizeX, int _sizeY, int _renderLayer = -1)
        {
            TilePixelSize = _tilePixelSize;
            Data = _data;
            Tiles = _tileArray;
            SizeX = _sizeX;
            SizeY = _sizeY;
            RenderLayer = _renderLayer;

            seed = 1337 * _sizeX + _tilePixelSize * _renderLayer;
            random = new Random(seed);
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

            TileRotations = new byte[Tiles.Length];
            for (int i = 0; i < Tiles.Length; i++)
            {
                TileRotations[i] = (byte)random.Next(0, 4);
            }

            isDirty = true;

            Game.OnUpdateEvent += RegenerateEntireMesh;
            RenderLayerManager.OrderRenderable(this);
        }

        /// <summary>
        /// Creates a <see cref="Tilemap"/> set up for rendering with one shared material.
        /// </summary>
        /// <param name="_tilePixelSize">The pixel size of each tile on the screen (Does not have anything to
        ///  do with the pixel size of the material's texture.)</param>
        /// <param name="_sizeX">The size of the Tilemap on the X axis.</param>
        /// <param name="_sizeY">The size of the Tilemap on the Y axis.</param>
        /// <param name="_cloneArrays">Whether the input arrays should be cloned before storing
        ///  to prevent data overwriting. Note: If the input arrays are being used for multiple tilemaps with shared
        ///   materials, ensure that this is enabled.</param>
        /// <returns></returns>
        public static Tilemap CreateSharedMaterial(Material _material, TileData[] _data, int[] _tileArray, int _tilePixelSize,
            int _sizeX, int _sizeY, int _renderLayer = -1, bool _cloneArrays = true)
        {
            TileData[] data = _data;
            int[] tiles = _tileArray;
            if (_cloneArrays)
            {
                data = (TileData[])_data.Clone();
                tiles = (int[])_tileArray.Clone();
            }
            else
            {
                Debug.LogWarning("Tilemap with shared material is being created without cloning input arrays.");
            }
            for (int i = 0; i < data.Length; i++)
            {
                data[i].Material = _material;
            }
            return new Tilemap(data, tiles, _tilePixelSize, _sizeX, _sizeY, _renderLayer);
        }

        /// <summary>
        /// Creates a <see cref="Tilemap"/> set up for rendering with multiple materials, and multiple renderers.
        /// </summary>
        /// <param name="_tilePixelSize">The pixel size of each tile on the screen (Does not have anything to
        ///  do with the pixel size of the material's texture.)</param>
        /// <param name="_sizeX">The size of the Tilemap on the X axis.</param>
        /// <param name="_sizeY">The size of the Tilemap on the Y axis.</param>
        /// <param name="_cloneArrays">Whether the input arrays should be cloned before storing
        ///  to prevent data overwriting.</param>
        /// <returns></returns>
        public static Tilemap CreateMultiMaterial(TileData[] _data, int[] _tileArray, int _tilePixelSize,
            int _sizeX, int _sizeY, int _renderLayer = -1, bool _cloneArrays = true)
        {
            TileData[] data = _data;
            int[] tiles = _tileArray;
            if (_cloneArrays)
            {
                data = (TileData[])_data.Clone();
                tiles = (int[])_tileArray.Clone();
            }
            return new Tilemap(data, tiles, _tilePixelSize, _sizeX, _sizeY, _renderLayer);
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

                float xPos = i % SizeX * realTilePixelSize;
                float yPos = i / SizeX * realTilePixelSize;

                // Finding tile and its 8 neighbors
                int[] neighbors = new int[9];
                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        int tileIndex = i + x + (y * SizeX);
                        if (tileIndex < Tiles.Length && tileIndex >= 0)
                        {
                            int neighborXPos = (i % SizeX) + x;
                            if(neighborXPos < 0 || neighborXPos >= SizeX)
                            {
                                // This neighbor tile is on another y-level, so mark it as blank instead
                                neighbors[(x + 1) + ((y + 1) * 3)] = -1;
                            }
                            else
                            {
                                // Check the actual tile
                                neighbors[(x + 1) + ((y + 1) * 3)] = Tiles[tileIndex];
                            }
                        }
                        else
                        {
                            // Index outside of tile array length
                            neighbors[(x+1) + ((y+1) * 3)] = -1;
                        }
                    }
                }

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
                    Vector2 newUV;
                    if(data.Ruleset != null)
                    {
                        newUV = data.Ruleset.CheckRulesetGetVertexUV(neighbors, new Vector2(u, v));
                    }
                    else
                    {
                        newUV = Spritesheets.spritesheets.ContainsKey(data.Material.MainTexture) ?
                            Spritesheets.GetVertexUV(data.Material.MainTexture,
                            data.SpriteColumn, data.SpriteRow, new Vector2(u, v)) :
                            new Vector2(u, v);
                        if (data.AllowRandomRotation)
                        {
                            newUV = RotateUV(newUV, TileRotations[i] * 90);
                        }
                    }

                    mesh.Vertices.Add(newUV.X); // U
                    mesh.Vertices.Add(newUV.Y); // V
                }

                // Indices
                int vertices = mesh.Vertices.Count / 4 - 4;
                mesh.Indices.Add((uint)(vertices + 0));
                mesh.Indices.Add((uint)(vertices + 1));
                mesh.Indices.Add((uint)(vertices + 2));
                mesh.Indices.Add((uint)(vertices + 0));
                mesh.Indices.Add((uint)(vertices + 2));
                mesh.Indices.Add((uint)(vertices + 3));
            }

            foreach (var m in meshDataDictionary)
            {
                if (m.Value.Vertices.Count > 0)
                {
                    m.Value.Renderer.SetVertexArrays(m.Value.Vertices.ToArray(), m.Value.Indices.ToArray(),
                        !m.Value.Renderer.HasVertexData, _setDirty: true);
                }
            }
        }

        private Vector2 RotateUV(Vector2 _uv, float _degrees)
        {
            float mid = 0.5f;
            _degrees *= MathF.PI / 180f;
            return new Vector2(
                MathF.Cos(_degrees) * (_uv.X - mid) + MathF.Sin(_degrees) * (_uv.Y - mid) + mid,
                MathF.Cos(_degrees) * (_uv.Y - mid) - MathF.Sin(_degrees) * (_uv.X - mid) + mid
            );
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
