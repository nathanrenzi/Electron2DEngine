using Electron2D.Core.Rendering;
using Electron2D.Core.UI;
using Electron2D.Core.UserInterface;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Build.Resources.Objects
{
    public class TestUi : UiComponent
    {
        public static readonly float[] vertices =
        {
            // Positions    UV            Color                     TexIndex
             1f,  1f,       1.0f, 1.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // top right - red
             1f, -1f,       1.0f, 0.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // bottom right - green
            -1f, -1f,       0.0f, 0.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // bottom left - blue
            -1f,  1f,       0.0f, 1.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // top left - white
        };

        public static readonly float[] defaultUV =
        {
            1.0f, 1.0f,
            1.0f, 0.0f,
            0.0f, 0.0f,
            0.0f, 1.0f,
        };

        public static readonly uint[] indices =
        {
            0, 1, 3, // Triangle 1
            1, 2, 3  // Triangle 2
        };

        public TestUi(Color _startColor, int _sizeX, int _sizeY)
            : base(_sizeX: _sizeX, _sizeY: _sizeY)
        {
            ResizeVertices();
            meshRenderer.SetVertexArrays(vertices, indices);

            Constraints.SetPosition(new PixelConstraint(20, UiConstraintSide.Left));
            Constraints.SetPosition(new PixelConstraint(20, UiConstraintSide.Bottom));
            SetColor(_startColor);
        }

        /// <summary>
        /// Resizes the vertex array to switch it from -1 to 1, to -size*2 to size*2 for rendering
        /// </summary>
        private void ResizeVertices()
        {
            int vertexStride = 9;
            float sx = SizeX * 2;
            float sy = SizeY * 2;
            int loops = vertices.Length / vertexStride;

            for (int i = 0; i < loops; i++)
            {
                vertices[0 + (i * vertexStride)] *= sx;
                vertices[1 + (i * vertexStride)] *= sy;
            }
        }

        protected override void OnUiEvent(UiEvent _event)
        {
            if (_event == UiEvent.LeftClickDown)
            {
                SetColor(Color.DarkGray);
            }
            else if (_event == UiEvent.LeftClickUp)
            {
                SetColor(ThisFrameData.isHovered ? Color.LightGray : Color.White);
            }

            if (_event == UiEvent.HoverStart)
            {
                if (!ThisFrameData.isLeftClicked)
                {
                    SetColor(Color.LightGray);
                }
            }
            else if (_event == UiEvent.HoverEnd)
            {
                if (!ThisFrameData.isLeftClicked)
                {
                    SetColor(Color.White);
                }
            }
        }
    }
}