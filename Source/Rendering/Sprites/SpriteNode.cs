using Atlas2D.Rendering;
using System.Numerics;

namespace Atlas2D
{
    public class SpriteNode : TransformNode, IRenderable
    {
        public MeshRenderer Renderer { get; private set; }
        public int RenderLayer { get; }
        public bool IgnorePostProcessing { get; }

        public Vector2 Size
        {
            get => _size;
            set { _size = value; RebuildMesh(); }
        }
        private Vector2 _size;

        public bool FlipX
        {
            get => _flipX;
            set { _flipX = value; RebuildMesh(); }
        }
        private bool _flipX;

        public bool FlipY
        {
            get => _flipY;
            set { _flipY = value; RebuildMesh(); }
        }
        private bool _flipY;

        // 4 vertices * (X + Y + U + V)
        private readonly float[] _vertices = new float[16];
        private readonly uint[] _indices = { 2, 1, 0, 3, 2, 0 };

        public SpriteNode(Vector2 size, SharedResource<Material> material, int renderLayer = 1, bool ignorePostProcessing = false)
        {
            _size = size;
            RenderLayer = renderLayer;
            IgnorePostProcessing = ignorePostProcessing;

            BuildMesh();

            Renderer = new MeshRenderer(this, material);
            Renderer.SetVertexArrays(_vertices, _indices);
        }

        protected override void OnEnable() => RenderLayerManager.OrderRenderable(this);

        protected override void OnDisable() => RenderLayerManager.RemoveRenderable(this);

        protected override void OnDispose()
        {
            Renderer?.Dispose();
            RenderLayerManager.RemoveRenderable(this);
        }

        private void BuildMesh()
        {
            float hw = _size.X / 2f;
            float hh = _size.Y / 2f;

            float uMin = _flipX ? 1f : 0f;
            float uMax = _flipX ? 0f : 1f;
            float vMin = _flipY ? 1f : 0f;
            float vMax = _flipY ? 0f : 1f;

            // Top Left
            _vertices[0] = -hw; _vertices[1] = hh;
            _vertices[2] = uMin; _vertices[3] = vMax;
            // Top Right
            _vertices[4] = hw;  _vertices[5] = hh;
            _vertices[6] = uMax; _vertices[7] = vMax;
            // Bottom Right
            _vertices[8] = hw;  _vertices[9] = -hh;
            _vertices[10] = uMax; _vertices[11] = vMin;
            // Bottom Left
            _vertices[12] = -hw; _vertices[13] = -hh;
            _vertices[14] = uMin; _vertices[15] = vMin;
        }

        private void RebuildMesh()
        {
            BuildMesh();
            if (Renderer?.IsLoaded == true)
                Renderer.SetVertexArrays(_vertices, _indices, false, true);
        }

        public void Render() => Renderer.Render();
    }
}
