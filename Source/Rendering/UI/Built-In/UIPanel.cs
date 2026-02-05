using Electron2D.Rendering;
using System.Drawing;

namespace Electron2D.UI
{
    public sealed class UIPanel : UIElement
    {
        private float[] _vertices = new float[16];
        private static readonly uint[] _indices =
        {
            3, 1, 0,
            3, 2, 1
        };

        public UIPanel(ITexture texture, int sizeX = 0, int sizeY = 0, int uiRenderLayer = 0, bool useScreenPosition = true, bool ignorePostProcessing = true)
            : base(sizeX, sizeY, uiRenderLayer, useScreenPosition, ignorePostProcessing, true)
        {
            Renderer.Material.MainTexture = texture;
            UpdateMesh();
            Renderer.SetVertexArrays(_vertices, _indices);
        }

        public UIPanel(Color color, int sizeX = 0, int sizeY = 0, int uiRenderLayer = 0, bool useScreenPosition = true, bool ignorePostProcessing = true)
            : base(sizeX, sizeY, uiRenderLayer, useScreenPosition, ignorePostProcessing, true)
        {
            SetColor(color);
            UpdateMesh();
            Renderer.SetVertexArrays(_vertices, _indices);
        }

        public UIPanel(Material material, int sizeX = 0, int sizeY = 0, int uiRenderLayer = 0, bool useScreenPosition = true, bool ignorePostProcessing = true)
            : base(sizeX, sizeY, uiRenderLayer, useScreenPosition, ignorePostProcessing, true)
        {
            Renderer.SetMaterial(material);
            UpdateMesh();
            Renderer.SetVertexArrays(_vertices, _indices);
        }

        public override void UpdateMesh()
        {
            float left = -Pivot.X * Size.X;
            float right = left + Size.X;
            float top = -Pivot.Y * Size.Y;
            float bottom = top + Size.Y;

            // Top Left
            _vertices[0] = left;
            _vertices[1] = top;
            _vertices[2] = 0f;
            _vertices[3] = 0f;

            // Top Right
            _vertices[4] = right;
            _vertices[5] = top;
            _vertices[6] = 1f;
            _vertices[7] = 0f;

            // Bottom Right
            _vertices[8] = right;
            _vertices[9] = bottom;
            _vertices[10] = 1f;
            _vertices[11] = 1f;

            // Bottom Left
            _vertices[12] = left;
            _vertices[13] = bottom;
            _vertices[14] = 0f;
            _vertices[15] = 1f;

            Renderer.IsVertexDirty = true;
        }
    }
}
