using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Electron2D.PhysicsBox2D;
using Electron2D.Rendering;
using System.Numerics;

namespace Electron2D
{
    public class Tilemap : IRenderable, IGameClass
    {
        public TileData[] Data { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }
        public int[] Tiles { get; set; }
        public byte[] TileRotations { get; set; }
        public uint CollisionBody { get; private set; } = 999999999;
        public Dictionary<Vector2, Fixture> CollisionFixtures { get; set; } = new();
        public Dictionary<Vector2, bool> CollisionFixtureUpdates { get; set; } = new();
        public Transform Transform { get; private set; }
        public int TilePixelSize { get; set; }
        public int RenderLayer { get; }
        public bool IgnorePostProcessing { get; } = false;

        private int _realTilePixelSize => TilePixelSize * 2;

        private Dictionary<Material, TileMesh> _meshDataDictionary = new();
        private Random _random;
        private int _seed;
        private bool _isDirty = false;
        private bool _isColliderDirty = false;

        private Tilemap(TileData[] data, int[] tileArray, int tilePixelSize,
            int sizeX, int sizeY, int renderLayer = -1)
        {
            TilePixelSize = tilePixelSize;
            Data = data;
            Tiles = tileArray;
            SizeX = sizeX;
            SizeY = sizeY;
            RenderLayer = renderLayer;

            _seed = 1337 * sizeX + tilePixelSize * renderLayer;
            _random = new Random(_seed);
            Transform = new Transform();

            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i].Material == null) continue;
                if (!_meshDataDictionary.ContainsKey(Data[i].Material.Value))
                    _meshDataDictionary.Add(Data[i].Material.Value, new TileMesh(Transform, Data[i].Material));
            }

            TileRotations = new byte[Tiles.Length];
            for (int i = 0; i < Tiles.Length; i++)
                TileRotations[i] = (byte)_random.Next(0, 4);

            _isDirty = true;
            _isColliderDirty = true;

            RenderLayerManager.OrderRenderable(this);
            Engine.Game.RegisterGameClass(this);
        }

        public static Tilemap CreateSharedMaterial(SharedResource<Material> material, TileData[] data,
            int[] tileArray, int tilePixelSize,
            int sizeX, int sizeY, int renderLayer = -1, bool cloneArrays = true)
        {
            TileData[] d = cloneArrays ? (TileData[])data.Clone() : data;
            int[] tiles = cloneArrays ? (int[])tileArray.Clone() : tileArray;

            if (!cloneArrays)
                Debug.LogWarning("Tilemap with shared material is being created without cloning input arrays.");

            for (int i = 0; i < d.Length; i++)
                d[i].Material = material;

            return new Tilemap(d, tiles, tilePixelSize, sizeX, sizeY, renderLayer);
        }

        public static Tilemap CreateMultiMaterial(TileData[] data, int[] tileArray, int tilePixelSize,
            int sizeX, int sizeY, int renderLayer = -1, bool cloneArrays = true)
        {
            TileData[] d = cloneArrays ? (TileData[])data.Clone() : data;
            int[] tiles = cloneArrays ? (int[])tileArray.Clone() : tileArray;
            return new Tilemap(d, tiles, tilePixelSize, sizeX, sizeY, renderLayer);
        }

        public void Update() { RegenerateEntireMesh(); }

        public void FixedUpdate() { }

        public void Dispose()
        {
            RenderLayerManager.RemoveRenderable(this);
            Engine.Game.UnregisterGameClass(this);
        }

        public string ToJson() => throw new NotImplementedException();
        public static Tilemap FromJson(string json) => throw new NotImplementedException();

        private void RegenerateEntireMesh()
        {
            if (!_isDirty && _isColliderDirty) RegenerateColliders();
            if (!_isDirty) return;
            _isDirty = false;

            for (int i = 0; i < Tiles.Length; i++)
            {
                if (Tiles[i] == -1) continue;
                TileData data = Data[Tiles[i]];
                TileMesh mesh = _meshDataDictionary[data.Material.Value];
                Texture2D mainTexture = data.Material.Value.MainTexture.Value;

                Vector2 pos = FromIndex(i);
                float xPos = pos.X * _realTilePixelSize;
                float yPos = pos.Y * _realTilePixelSize;

                int[] neighbors = new int[9];
                for (int y = -1; y < 2; y++)
                {
                    for (int x = -1; x < 2; x++)
                    {
                        int tileIndex = i + x + (y * SizeX);
                        if (tileIndex < Tiles.Length && tileIndex >= 0)
                        {
                            int neighborXPos = (i % SizeX) + x;
                            neighbors[(x + 1) + ((y + 1) * 3)] = (neighborXPos < 0 || neighborXPos >= SizeX)
                                ? -1
                                : Tiles[tileIndex];
                        }
                        else
                        {
                            neighbors[(x + 1) + ((y + 1) * 3)] = -1;
                        }
                    }
                }

                for (int a = 0; a < 4; a++)
                {
                    float xMod = (a == 0 || a == 3) ? _realTilePixelSize : 0;
                    float yMod = (a == 0 || a == 1) ? _realTilePixelSize : 0;

                    mesh.Vertices.Add(xPos + xMod);
                    mesh.Vertices.Add(yPos + yMod);

                    float u = xMod / _realTilePixelSize;
                    float v = yMod / _realTilePixelSize;
                    Vector2 newUV;
                    if (data.Ruleset != null)
                    {
                        newUV = data.Ruleset.CheckRulesetGetVertexUV(neighbors, new Vector2(u, v));
                    }
                    else
                    {
                        newUV = Spritesheets.spritesheets.ContainsKey(mainTexture)
                            ? Spritesheets.GetVertexUV(mainTexture, data.SpriteColumn, data.SpriteRow, new Vector2(u, v))
                            : new Vector2(u, v);

                        if (data.AllowRandomRotation)
                            newUV = RotateUV(newUV, TileRotations[i] * 90);
                    }

                    mesh.Vertices.Add(newUV.X);
                    mesh.Vertices.Add(newUV.Y);
                }

                int vertices = mesh.Vertices.Count / 4 - 4;
                mesh.Indices.Add((uint)(vertices + 0));
                mesh.Indices.Add((uint)(vertices + 1));
                mesh.Indices.Add((uint)(vertices + 2));
                mesh.Indices.Add((uint)(vertices + 0));
                mesh.Indices.Add((uint)(vertices + 2));
                mesh.Indices.Add((uint)(vertices + 3));
            }

            foreach (var m in _meshDataDictionary)
            {
                if (m.Value.Vertices.Count > 0)
                {
                    m.Value.Renderer.SetVertexArrays(m.Value.Vertices.ToArray(), m.Value.Indices.ToArray(),
                        !m.Value.Renderer.HasVertexData, _setDirty: true);
                }
            }

            RegenerateColliders();
        }

        private void RegenerateColliders()
        {
            if (!_isColliderDirty) return;
            _isColliderDirty = false;

            Body body = null;
            foreach (var entry in CollisionFixtureUpdates)
            {
                Vector2 localPosition = entry.Key;
                bool add = entry.Value;

                if (add)
                {
                    Vector2 fixturePosition = (localPosition + (Vector2.One * 0.5f)) * SizeX / Physics.WorldScalar;
                    FixtureDef fdef = new FixtureDef();
                    PolygonShape shape = new PolygonShape();
                    shape.SetAsBox(TilePixelSize / Physics.WorldScalar, TilePixelSize / Physics.WorldScalar, fixturePosition, 0);
                    fdef.shape = shape;

                    if (CollisionBody == 999999999)
                    {
                        BodyDef bodyDef = new BodyDef() { position = Transform.Position / Physics.WorldScalar };
                        CollisionBody = Physics.CreatePhysicsBody(bodyDef, fdef, new MassData(), true);
                    }

                    body ??= Physics.GetBody(CollisionBody);
                    CollisionFixtures.Add(localPosition, body.CreateFixture(fdef));
                }
                else
                {
                    CollisionFixtures.Remove(localPosition);
                }
            }
            CollisionFixtureUpdates.Clear();
        }

        private Vector2 RotateUV(Vector2 uv, float degrees)
        {
            float mid = 0.5f;
            degrees *= MathF.PI / 180f;
            return new Vector2(
                MathF.Cos(degrees) * (uv.X - mid) + MathF.Sin(degrees) * (uv.Y - mid) + mid,
                MathF.Cos(degrees) * (uv.Y - mid) - MathF.Sin(degrees) * (uv.X - mid) + mid
            );
        }

        public void SetTileID(int x, int y, byte tileID)
        {
            int index = ToIndex(x, y);
            int currentTile = Tiles[index];
            if (!(Data[currentTile].IsCollider && Data[tileID].IsCollider))
                _isColliderDirty = true;
            Tiles[index] = tileID;
            _isDirty = true;
        }

        public int GetTileID(int x, int y) => Tiles[ToIndex(x, y)];
        public TileData GetTileData(int x, int y) => Data[GetTileID(x, y)];
        private int ToIndex(int x, int y) => x + y * SizeX;
        private Vector2 FromIndex(int index) => new Vector2(index % SizeX, index / SizeX);


        public void Render()
        {
            foreach (var m in _meshDataDictionary)
            }
        }
        }
    }
}