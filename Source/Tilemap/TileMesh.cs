using Electron2D.Rendering;

namespace Electron2D
{
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
}
