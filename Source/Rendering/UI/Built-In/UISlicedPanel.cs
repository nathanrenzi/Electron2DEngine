using Electron2D.Rendering;
using System.Numerics;

namespace Electron2D.UI
{
    /// <summary>
    /// A <see cref="UIElement"/> that renders a 9-sliced panel, procedurally stretching a texture's edges and corners
    /// to maintain consistent border scale at any size.
    /// </summary>
    public sealed class UISlicedPanel : UIElement
    {
        private float[] _vertices = new float[16 * 4];

        private float[] _defaultUV = new float[16 * 2];

        private static readonly uint[] _indices =
        {
            // TL corner
             1,  0,  4,
             1,  4,  5,
 
            // TR corner
             3,  2,  6,
             3,  6,  7,
 
            // BL corner
             9,  8, 12,
             9, 12, 13,
 
            // BR corner
            11, 10, 14,
            11, 14, 15,
 
            // Top side
             2,  1,  5,
             2,  5,  6,
 
            // Right side
             7,  6, 10,
             7, 10, 11,
 
            // Bottom side
            10,  9, 13,
            10, 13, 14,
 
            // Left side
             5,  4,  8,
             5,  8,  9,
 
            // Middle
             6,  5,  9,
             6,  9, 10,
        };

        private float _left;
        private float _right;
        private float _top;
        private float _bottom;
        private int _borderPixelSize;
        private int _stride = 4;

        public UISlicedPanel(SharedResource<Texture2D> texture, Border borderUV, int borderPixelSize, UIRenderArgs? arguments = null)
            : base(arguments, true)
        {
            _left = borderUV.Left;
            _right = borderUV.Right;
            _top = borderUV.Top;
            _bottom = borderUV.Bottom;
            _borderPixelSize = borderPixelSize;

            UpdateMesh();

            Renderer.Material.Value.SetMainTexture(texture);
            Renderer.SetVertexArrays(_vertices, _indices);
        }

        public UISlicedPanel(SharedResource<Material> material, Border borderUV, int borderPixelSize, UIRenderArgs? arguments = null)
            : base(arguments, true)
        {
            _left = borderUV.Left;
            _right = borderUV.Right;
            _top = borderUV.Top;
            _bottom = borderUV.Bottom;
            _borderPixelSize = borderPixelSize;

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

            // The positions of the inner border
            float L2 = L1 + border;
            float R2 = R1 - border;
            float T2 = T1 + border;
            float B2 = B1 - border;

            // UV coordinates for the border seams
            Vector2 texSize = Renderer.Material.Value.MainTexture.Value.GetSize();
            float offsetTexelU = 0.1f / texSize.X;
            float offsetTexelV = 0.1f / texSize.Y;
            float LU = Math.Clamp(_left, 0, 1f) + offsetTexelU;
            float RU = 1 - Math.Clamp(_right, 0, 1f) - offsetTexelU;
            float TV = 1 - Math.Clamp(_top, 0, 1f) - offsetTexelV;
            float BV = Math.Clamp(_bottom, 0, 1f) + offsetTexelV;
            float U0 = offsetTexelU;
            float U1 = 1 - offsetTexelU;
            float V0 = offsetTexelV;
            float V1 = 1 - offsetTexelV;

            // 4x4 grid of vertices, row by row:
            //  0  1  2  3    (y = T1)
            //  4  5  6  7    (y = T2)
            //  8  9 10 11    (y = B2)
            // 12 13 14 15    (y = B1)
            SetVertex(0, L1, T1, U0, V1);
            SetVertex(1, L2, T1, LU, V1);
            SetVertex(2, R2, T1, RU, V1);
            SetVertex(3, R1, T1, U1, V1);
            SetVertex(4, L1, T2, U0, TV);
            SetVertex(5, L2, T2, LU, TV);
            SetVertex(6, R2, T2, RU, TV);
            SetVertex(7, R1, T2, U1, TV);
            SetVertex(8, L1, B2, U0, BV);
            SetVertex(9, L2, B2, LU, BV);
            SetVertex(10, R2, B2, RU, BV);
            SetVertex(11, R1, B2, U1, BV);
            SetVertex(12, L1, B1, U0, V0);
            SetVertex(13, L2, B1, LU, V0);
            SetVertex(14, R2, B1, RU, V0);
            SetVertex(15, R1, B1, U1, V0);

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
