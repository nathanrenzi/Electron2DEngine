using Electron2D.Core.Management.Textures;
using Electron2D.Core.Rendering;
using System.Numerics;

namespace Electron2D.Core
{
    /// <summary>
    /// Advanced version of <see cref="MeshRenderer"/> that allows for rendering sprites on a spritesheet.
    /// </summary>
    public class SpriteRenderer : MeshRenderer
    {
        private readonly float[] vertices =
        {
             1f,  1f,       1.0f, 1.0f,
             1f, -1f,       1.0f, 0.0f,
            -1f, -1f,       0.0f, 0.0f,
            -1f,  1f,       0.0f, 1.0f
        };

        private static readonly float[] defaultUV =
        {
            1.0f, 1.0f,
            1.0f, 0.0f,
            0.0f, 0.0f,
            0.0f, 1.0f,
        };

        private static readonly uint[] indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        public int CurrentColumn { get; private set; }
        public int CurrentRow { get; private set; }

        public SpriteRenderer(Transform _transform, Material _material, int _renderLayer = 1) : base(_transform, _material)
        {
            RenderLayer = _renderLayer;

            // Must be called in order for HasVertexData to be true
            SetVertexArrays(vertices, indices);
        }

        /// <summary>
        /// Returns the default texture UV associated with the vertex inputted.
        /// </summary>
        /// <param name="_vertex">The vertex to get the UV of.</param>
        /// <returns></returns>
        public Vector2 GetDefaultUV(int _vertex = 0)
        {
            return new Vector2(defaultUV[_vertex * 2], defaultUV[(_vertex * 2) + 1]);
        }
    }
}
