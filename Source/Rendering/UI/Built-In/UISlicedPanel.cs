using Electron2D.Rendering;

namespace Electron2D.UI
{
    /// <summary>
    /// A UI Component that can procedurally stretch a texture along it's borders, maintaining the same scale at any size.
    /// </summary>
    public sealed class UISlicedPanel : UIElement
    {
        private float[] _vertices = new float[36 * 4];

        private static float[] _defaultUV = new float[36 * 2];

        private static readonly uint[] _indices =
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

        private float _left;
        private float _right;
        private float _top;
        private float _bottom;
        private int _borderPixelSize;
        private int _stride = 4;

        public UISlicedPanel(ITexture texture, Border borderUV, int borderPixelSize, UIRenderArgs? arguments = null)
            : base(arguments, true)
        {
            _left = borderUV.Left;
            _right = borderUV.Right;
            _top = borderUV.Top;
            _bottom = borderUV.Bottom;
            _borderPixelSize = borderPixelSize * 2;

            UpdateMesh();

            Renderer.Material.MainTexture = texture;
            Renderer.SetVertexArrays(_vertices, _indices);
        }

        public UISlicedPanel(Material material, Border borderUV, int borderPixelSize, UIRenderArgs? arguments = null)
            : base(arguments, true)
        {
            _left = borderUV.Left;
            _right = borderUV.Right;
            _top = borderUV.Top;
            _bottom = borderUV.Bottom;
            _borderPixelSize = borderPixelSize * 2;

            UpdateMesh();

            Renderer.SetMaterial(material);
            Renderer.SetVertexArrays(_vertices, _indices);
        }

        public override void UpdateMesh()
        {
            // The positions of the outer vertices
            Rect rect = GetLocalBounds();
            float L1 = rect.X;
            float R1 = rect.X + rect.Width;
            float T1 = rect.Y;
            float B1 = rect.Y + rect.Height;

            // Scaling border to prevent overlap
            float width = R1 - L1;
            float height = B1 - T1;
            float maxTotalBorder = Math.Min(width, height);
            float scale = Math.Min(1f, maxTotalBorder / (_borderPixelSize * 2f));
            float border = _borderPixelSize * scale;

            // The positions of the padding
            float L2 = L1 + border;
            float R2 = R1 - border;
            float T2 = T1 + border;
            float B2 = B1 - border;

            // Creating the UV coordinates for the non-0 and non-1 UV values that should be the same regardless of the size of UI
            float LU = Math.Clamp(_left, 0, 1f);
            float RU = 1 - Math.Clamp(_right, 0, 1f);
            float TV = Math.Clamp(_top, 0, 1f);
            float BV = 1 - Math.Clamp(_bottom, 0, 1f);

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
            SetVertex(27, L2, B1, LU, 0);
            SetVertex(28, L1, T2, 0, TV);
            SetVertex(29, L2, T2, LU, TV);
            SetVertex(30, L2, B2, LU, BV);
            SetVertex(31, L1, B2, 0, BV);
            SetVertex(32, L2, T2, LU, TV);
            SetVertex(33, R2, T2, RU, TV);
            SetVertex(34, R2, B2, RU, BV);
            SetVertex(35, L2, B2, LU, BV);

            InitializeDefaultUVArray();

            Renderer.IsVertexDirty = true;
        }

        private void SetVertex(int index, float x, float y, float u, float v)
        {
            _vertices[index * _stride + 0] = x;
            _vertices[index * _stride + 1] = y;
            _vertices[index * _stride + 2] = u;
            _vertices[index * _stride + 3] = v;
        }

        /// <summary>
        /// Used after initializing the vertex array. Grabs the UV's and stores them separately for texture coordinate calculation.
        /// </summary>
        private void InitializeDefaultUVArray()
        {
            int loops = _vertices.Length / _stride;
            for (int i = 0; i < loops; i++)
            {
                _defaultUV[i * 2 + 0] = _vertices[i * _stride + (int)MeshVertexAttribute.UvX];
                _defaultUV[i * 2 + 1] = _vertices[i * _stride + (int)MeshVertexAttribute.UvY];
            }
        }
    }
}
