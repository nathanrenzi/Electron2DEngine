using Atlas2D.Rendering;

namespace Atlas2D
{
    public struct TileMesh
    {
        public List<float> Vertices;
        public List<uint> Indices;
        public MeshRenderer Renderer;

        public TileMesh(Transform transform, SharedResource<Material> material)
        {
            Vertices = new List<float>();
            Indices = new List<uint>();
            Renderer = new MeshRenderer(transform, material);
        }
    }
}
