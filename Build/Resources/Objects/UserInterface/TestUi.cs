using Electron2D.Core.Rendering;
using Electron2D.Core.UI;
using Electron2D.Core.UserInterface;
using System.Drawing;

namespace Electron2D.Build.Resources.Objects
{
    public class TestUi : UiComponent
    {
        private Color startColor;

        public TestUi(Color _startColor) : base()
        {
            constraints.SetPosition(new PixelConstraint(20, UiConstraintSide.Left));
            constraints.SetPosition(new PixelConstraint(20, UiConstraintSide.Bottom));
            startColor = _startColor;
        }

        private void SetColor(Color _color)
        {
            rendererReference.SetVertexValueAll((int)VertexAttribute.ColorR, _color.R / 255f);
            rendererReference.SetVertexValueAll((int)VertexAttribute.ColorG, _color.G / 255f);
            rendererReference.SetVertexValueAll((int)VertexAttribute.ColorB, _color.B / 255f);
            rendererReference.SetVertexValueAll((int)VertexAttribute.ColorA, _color.A / 255f);
        }

        protected override void OnUiEvent(UiEvent _event)
        {
            if(_event == UiEvent.Load) SetColor(startColor);
            if(_event == UiEvent.LeftClickDown)
            {
                sizeX = 210;
                sizeY = 110;
                GenerateMesh();

                SetColor(Color.Sienna);
            }
            else if(_event == UiEvent.LeftClickUp)
            {
                sizeX = 200;
                sizeY = 100;
                GenerateMesh();

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