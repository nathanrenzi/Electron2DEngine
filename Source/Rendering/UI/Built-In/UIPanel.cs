using Electron2D.Rendering;
using System.Drawing;

namespace Electron2D.UI
{
    /// <summary>
    /// A simple UI element that renders a solid color, texture, or material as a quad.
    /// </summary>
    public sealed class UIPanel : UIElement
    {
        private float[] _vertices = new float[16];
        private static readonly uint[] _indices =
        {
            3, 1, 0,
            3, 2, 1
        };

        /// <summary>
        /// Creates a <see cref="UIPanel"/> rendered with a texture.
        /// </summary>
        public UIPanel(SharedResource<Texture2D> texture, UIRenderArgs? arguments = null) : base(arguments, true)
        {
            Renderer.Material.Value.SetMainTexture(texture);
            Initialize();
        }

        public UIPanel(Color color, UIRenderArgs? arguments = null) : base(arguments, true)
        {
            SetColor(color);
            Initialize();
        }

        public UIPanel(SharedResource<Material> material, UIRenderArgs? arguments = null) : base(arguments, true)
        {
            Renderer.SetMaterial(material);
            Initialize();
        }

        private void Initialize()
        {
            UpdateMesh();
            Renderer.SetVertexArrays(_vertices, _indices);
        }

        public override void UpdateMesh()
        {
            Rect rect = GetLocalBounds();
            float left = rect.X;
            float right = rect.X + rect.Width;
            float top = rect.Y;
            float bottom = rect.Y + rect.Height;

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
