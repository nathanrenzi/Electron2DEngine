using Electron2D.Core.Rendering;
using Electron2D.Core.UI;
using Electron2D.Core.UserInterface;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Build.Resources.Objects
{
    public class TestUi : UiComponent
    {
        private Color startColor;

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

        public TestUi(Color _startColor, int _sizeX, int _sizeY) : base(_vertices: vertices, _indices: indices, _defaultUV: defaultUV, _sizeX: _sizeX, _sizeY: _sizeY)
        {
            constraints.SetPosition(new PixelConstraint(20, UiConstraintSide.Left));
            constraints.SetPosition(new PixelConstraint(20, UiConstraintSide.Bottom));
            startColor = _startColor;
            SetColor(_startColor);
        }

        protected override void OnUiEvent(UiEvent _event)
        {
            if(_event == UiEvent.Load) SetColor(startColor);
            if(_event == UiEvent.LeftClickDown)
            {
                transform.scale = Vector2.One * 1.1f;
                SetColor(Color.Sienna);
            }
            else if(_event == UiEvent.LeftClickUp)
            {
                transform.scale = Vector2.One;

                SetColor(thisFrameData.isHovered ? Color.Wheat : Color.LightSkyBlue);
            }

            if(_event == UiEvent.HoverStart)
            {
                if (!thisFrameData.isLeftClicked)
                {
                    SetColor(Color.Wheat);
                }
            }
            else if(_event == UiEvent.HoverEnd)
            {
                if (!thisFrameData.isLeftClicked)
                {
                    SetColor(Color.LightSkyBlue);
                }
            }
        }
    }
}