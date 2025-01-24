using Electron2D.Rendering;

namespace Electron2D.UserInterface
{
    /// <summary>
    /// A UI Component that can procedurally stretch a texture along it's borders, maintaining the same scale at any size.
    /// </summary>
    public class SlicedPanel : UiComponent
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

        private float left;
        private float right;
        private float top;
        private float bottom;
        private float borderPixelSize;

        private int stride = 4; // This should be equal to how many floats are associated with each vertex

        /// <param name="_sizeX">The starting size on the X axis.</param>
        /// <param name="_sizeY">The starting size on the Y axis.</param>
        public SlicedPanel(Material _material, int _sizeX, int _sizeY, SlicedPanelDef _def,
            int _uiRenderLayer = 0, bool _ignorePostProcessing = false)
            : base(_ignorePostProcessing, _uiRenderLayer, sizeX: _sizeX, sizeY: _sizeY)
        {
            left = _def.Left;
            right = _def.Right;
            top = _def.Top;
            bottom = _def.Bottom;
            borderPixelSize = _def.BorderPixelSize * 2;

            BuildVertexMesh();
            // Indices are pre-written, so they are not generated at runtime

            Renderer.SetVertexArrays(vertices, indices);
            Renderer.SetMaterial(_material);
        }

        /// <summary>
        /// Rebuilds the mesh of the UI to commit any changes of the sizeX/Y to the vertex array.
        /// </summary>
        public void RebuildMesh()
        {
            BuildVertexMesh();
            Renderer.SetVertexArrays(vertices, indices, false);
            Renderer.IsVertexDirty = true;
        }

        /// <summary>
        /// Builds 9-sliced vertex arrays based on size and edge padding
        /// </summary>
        private void BuildVertexMesh()
        {
            // The positions of the outer vertices
            float L1 = -SizeX + (Anchor.X * -SizeX);
            float R1 = SizeX + (Anchor.X * -SizeX);
            float T1 = SizeY + (Anchor.Y * -SizeY);
            float B1 = -SizeY + (Anchor.Y * -SizeY);

            // The positions of the padding
            float L2 = L1 + borderPixelSize;
            float R2 = R1 - borderPixelSize;
            float T2 = T1 - borderPixelSize;
            float B2 = B1 + borderPixelSize;

            // Creating the UV coordinates for the non-0 and non-1 UV values that should be the same regardless of the size of UI
            float LU = Math.Clamp(left, 0, 1f);
            float RU = 1 - Math.Clamp(right, 0, 1f);
            float TV = 1 - Math.Clamp(top, 0, 1f);
            float BV = Math.Clamp(bottom, 0, 1f);

            SetVertex(0, L1, T1, 0, 1);
            SetVertex(1, L2, T1, LU, 1);
            SetVertex(2, L2, T2, LU, TV);
            SetVertex(3, L1, T2, 0, TV);
            SetVertex(4, R2, T1, RU, 1);
            SetVertex(5, R1, T1, 1, 1);
            SetVertex(6, R1, T2, 1, TV);
            SetVertex(7, R2, T2, RU, TV);
            SetVertex(8, R2, B2, RU, BV);
            SetVertex(9, R1, B2, 1, BV);
            SetVertex(10, R1, B1, 1, 0);
            SetVertex(11, R2, B1, RU, 0);
            SetVertex(12, L1, B2, 0, BV);
            SetVertex(13, L2, B2, LU, BV);
            SetVertex(14, L2, B1, LU, 0);
            SetVertex(15, L1, B1, 0, 0);
            SetVertex(16, L2, T1, LU, 1);
            SetVertex(17, R2, T1, RU, 1);
            SetVertex(18, R2, T2, RU, TV);
            SetVertex(19, L2, T2, LU, TV);
            SetVertex(20, R2, T2, RU, TV);
            SetVertex(21, R1, T2, 1, TV);
            SetVertex(22, R1, B2, 1, BV);
            SetVertex(23, R2, B2, RU, BV);
            SetVertex(24, L2, B2, LU, BV);
            SetVertex(25, R2, B2, RU, BV);
            SetVertex(26, R2, B1, RU, 0);
            SetVertex(27, L2, B1, LU, 0 );
            SetVertex(28, L1, T2, 0, TV);
            SetVertex(29, L2, T2, LU, TV);
            SetVertex(30, L2, B2, LU, BV);
            SetVertex(31, L1, B2, 0, BV);
            SetVertex(32, L2, T2, LU, TV);
            SetVertex(33, R2, T2, RU, TV);
            SetVertex(34, R2, B2, RU, BV);
            SetVertex(35, L2, B2, LU, BV);

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

        protected override void OnUiEvent(UiEvent _event)
        {
            switch (_event)
            {
                case UiEvent.Resize:
                    if(Renderer != null)
                    {
                        RebuildMesh();
                    }
                    break;
                case UiEvent.Anchor:
                    if(Renderer != null)
                    {
                        RebuildMesh();
                    }
                    break;
            }
        }
    }
}