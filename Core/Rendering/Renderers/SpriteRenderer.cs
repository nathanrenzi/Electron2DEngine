using Electron2D.Core.GameObjects;
using Electron2D.Core.Management.Textures;
using System.Numerics;

namespace Electron2D.Core.Rendering
{
    /// <summary>
    /// A renderer specializing in displaying images, and using spritesheets.
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

        public SpriteRenderer(Transform _transform, Material _material) : base(_transform, _material)
        {
            // Must be called in order for HasVertexData to be true
            SetVertexArrays(vertices, indices, defaultUV);
        }

        public void SetSubSprite(int _column, int _row)
        {
            if (!HasVertexData) return;

            CurrentColumn = _column;
            CurrentRow = _row;

            int loops = vertices.Length / layout.GetRawStride();
            Vector2 newUV;
            for (int i = 0; i < loops; i++)
            {
                // Getting the new UV from the spritesheet
                newUV = SpritesheetManager.GetVertexUV(Material.MainTexture, _column, _row, GetDefaultUV(i));

                // Setting the new UV
                vertices[(i * layout.GetRawStride()) + (int)MeshVertexAttribute.UvX] = newUV.X;
                vertices[(i * layout.GetRawStride()) + (int)MeshVertexAttribute.UvY] = newUV.Y;
            }

            IsDirty = true;
        }
    }
}
