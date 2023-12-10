using Electron2D.Core.Rendering;
using Electron2D.Core.UI;
using System.Drawing;

namespace Electron2D.Core.UserInterface
{
    /// <summary>
    /// The most basic UI component. Displays only a color / image.
    /// </summary>
    public class Panel : UiComponent
    {
        private static readonly float[] defaultVertices =
        {
            // Positions    UV
             1f,  1f,       1.0f, 1.0f,     // top right - red
             1f, -1f,       1.0f, 0.0f,     // bottom right - green
            -1f, -1f,       0.0f, 0.0f,     // bottom left - blue
            -1f,  1f,       0.0f, 1.0f,     // top left - white
        };

        public readonly float[] vertices =
{
            // Positions    UV
             1f,  1f,       1.0f, 1.0f,     // top right - red
             1f, -1f,       1.0f, 0.0f,     // bottom right - green
            -1f, -1f,       0.0f, 0.0f,     // bottom left - blue
            -1f,  1f,       0.0f, 1.0f,     // top left - white
        };

        public static readonly uint[] indices =
        {
            3, 1, 0, // Triangle 1
            3, 2, 1  // Triangle 2
        };

        public Panel(Color _mainColor, int _uiRenderLayer = 0, int _sizeX = 100, int _sizeY = 100,
            bool _useScreenPosition = true) : base(_uiRenderLayer, _sizeX, _sizeY, _useScreenPosition: _useScreenPosition)
        {
            UpdateMesh();
            SetColor(_mainColor);
        }
        public Panel(Material _material, int _uiRenderLayer = 0, int _sizeX = 100, int _sizeY = 100,
            bool _useScreenPosition = true) : base(_uiRenderLayer, _sizeX, _sizeY, _useScreenPosition: _useScreenPosition)
        {
            UpdateMesh();
            meshRenderer.SetMaterial(_material);
        }

        /// <summary>
        /// Resizes the vertex array to switch it from -1 to 1, to -size*2 to size*2 for rendering
        /// </summary>
        private void ResizeVertices()
        {
            int vertexStride = 4;
            float sx = SizeX;
            float sy = SizeY;
            int loops = vertices.Length / vertexStride;

            for (int i = 0; i < loops; i++)
            {
                vertices[0 + (i * vertexStride)] = defaultVertices[0 + (i * vertexStride)] * sx;
                vertices[1 + (i * vertexStride)] = defaultVertices[1 + (i * vertexStride)] * sy;
            }
        }

        public override void UpdateMesh()
        {
            ResizeVertices();
            meshRenderer.SetVertexArrays(vertices, indices);
        }
    }
}
