using Electron2D.Core.Rendering;
using Electron2D.Core.UserInterface;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Core.UserInterface
{
    /// <summary>
    /// A UI Component that can procedurally stretch a texture along it's borders, maintaining the same scale at any size.
    /// </summary>
    public class SlicedUiComponent : UiComponent
    {
        private float[] vertices = new float[36 * 4];

        private static float[] defaultUV = new float[36 * 2];

        private static readonly uint[] indices = // 18 tris * 3 uints
        {
            // Corners
            1, 0, 3,
            1, 2, 3,

            5, 4, 7,
            5, 7, 6,

            9, 8, 11,
            9, 11, 10,

            13, 12, 15,
            13, 15, 14,

            // Sides
            17, 16, 19,
            17, 19, 18,

            21, 20, 23,
            21, 23, 22,

            25, 24, 27,
            25, 27, 26,

            29, 28, 31,
            29, 31, 30,

            // Middle
            33, 32, 35,
            33, 35, 34
        };

        private int left;
        private int right;
        private int top;
        private int bottom;
        private float imageSize;
        private float borderScale;

        private int stride = 4; // This should be equal to how many floats are associated with each vertex

        /// <param name="_sizeX">The starting size on the X axis.</param>
        /// <param name="_sizeY">The starting size on the Y axis.</param>
        /// <param name="_left">The left padding of the 9-sliced texture.</param>
        /// <param name="_right">The right padding of the 9-sliced texture.</param>
        /// <param name="_top">The top padding of the 9-sliced texture.</param>
        /// <param name="_bottom">The bottom padding of the 9-sliced texture.</param>
        /// <param name="_imageSize">The size of the input image.</param>
        /// <param name="_borderScale">The scale of the border.</param>
        public SlicedUiComponent(Material _material, int _sizeX, int _sizeY,
            int _left, int _right, int _top, int _bottom, float _imageSize, float _borderScale = 1f)
            : base(_sizeX: _sizeX, _sizeY: _sizeY)
        {
            left = _left;
            right = _right;
            top = _top;
            bottom = _bottom;
            imageSize = _imageSize;
            borderScale = _borderScale;

            BuildVertexMesh();
            // Indices are pre-written, so they are not generated at runtime

            meshRenderer.SetVertexArrays(vertices, indices);
            meshRenderer.SetMaterial(_material);
        }

        /// <summary>
        /// Rebuilds the mesh of the UI to commit any changes of the sizeX/Y to the vertex array.
        /// </summary>
        public void RebuildMesh()
        {
            BuildVertexMesh();
            meshRenderer.SetVertexArrays(vertices, indices, false);
            meshRenderer.IsVertexDirty = true;
        }

        /// <summary>
        /// Builds 9-sliced vertex arrays based on size and edge padding
        /// </summary>
        private void BuildVertexMesh()
        {
            float sx = SizeX;
            float sy = SizeY;

            // The positions of the padding
            float L = -sx + left * borderScale;
            float R = sx - right * borderScale;
            float T = sy - top * borderScale;
            float B = -sy + bottom * borderScale;

            float ratioX = sx / imageSize;
            float ratioY = sy / imageSize;

            // Creating the UV coordinates for the non-0 and non-1 UV values that should be the same regardless of the size of UI
            float LU = Math.Clamp(left * ratioX / sx, 0, 1f);
            float RU = 1 - Math.Clamp(right * ratioX / sx, 0, 1f);
            float TV = 1 - Math.Clamp(top * ratioY / sy, 0, 1f);
            float BV = Math.Clamp(bottom * ratioY / sy, 0, 1f);

            SetVertex(0, -sx, sy, 0, 1);
            SetVertex(1, L, sy, LU, 1);
            SetVertex(2, L, T, LU, TV);
            SetVertex(3, -sx, T, 0, TV);
            SetVertex(4, R, sy, RU, 1);
            SetVertex(5, sx, sy, 1, 1);
            SetVertex(6, sx, T, 1, TV);
            SetVertex(7, R, T, RU, TV);
            SetVertex(8, R, B, RU, BV);
            SetVertex(9, sx, B, 1, BV);
            SetVertex(10, sx, -sy, 1, 0);
            SetVertex(11, R, -sy, RU, 0);
            SetVertex(12, -sx, B, 0, BV);
            SetVertex(13, L, B, LU, BV);
            SetVertex(14, L, -sy, LU, 0);
            SetVertex(15, -sx, -sy, 0, 0);
            SetVertex(16, L, sy, LU, 1);
            SetVertex(17, R, sy, RU, 1);
            SetVertex(18, R, T, RU, TV);
            SetVertex(19, L, T, LU, TV);
            SetVertex(20, R, T, RU, TV);
            SetVertex(21, sx, T, 1, TV);
            SetVertex(22, sx, B, 1, BV);
            SetVertex(23, R, B, RU, BV);
            SetVertex(24, L, B, LU, BV);
            SetVertex(25, R, B, RU, BV);
            SetVertex(26, R, -sy, RU, 0);
            SetVertex(27, L, -sy, LU, 0 );
            SetVertex(28, -sx, T, 0, TV);
            SetVertex(29, L, T, LU, TV);
            SetVertex(30, L, B, LU, BV);
            SetVertex(31, -sx, B, 0, BV);
            SetVertex(32, L, T, LU, TV);
            SetVertex(33, R, T, RU, TV);
            SetVertex(34, R, B, RU, BV);
            SetVertex(35, L, B, LU, BV);

            InitializeDefaultUVArray();
        }

        private void SetVertex(int _index, float _x, float _y, float _u, float _v)
        {
            vertices[_index * stride + 0] = _x;
            vertices[_index * stride + 1] = _y;
            vertices[_index * stride + 2] = _u;
            vertices[_index * stride + 3] = _v;
        }

        /// <summary>
        /// Used after initializing the vertex array. Grabs the UV's and stores them separately for texture coordinate calculation.
        /// </summary>
        private void InitializeDefaultUVArray()
        {
            int loops = vertices.Length / stride;
            for (int i = 0; i < loops; i++)
            {
                defaultUV[i * 2 + 0] = vertices[i * stride + (int)MeshVertexAttribute.UvX];
                defaultUV[i * 2 + 1] = vertices[i * stride + (int)MeshVertexAttribute.UvY];
            }
        }
    }
}