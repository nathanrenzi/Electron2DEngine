using Electron2D.Core.Rendering;
using Electron2D.Core.UI;
using Electron2D.Core.UserInterface;
using System.Drawing;

namespace Electron2D.Build.Resources.Objects
{
    public class TestUi : UiComponent
    {
        public TestUi() : base()
        {
            constraints.SetPosition(new PixelConstraint(20, UiConstraintSide.Left));
            constraints.SetPosition(new PixelConstraint(20, UiConstraintSide.Bottom));
        }

        protected override void OnUiEvent(UiEvent _event)
        {
            if(_event == UiEvent.LeftClickDown)
            {
                sizeX = 210;
                sizeY = 110;
                GenerateMesh();

                Color col = Color.Sienna;
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorR, col.R / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorG, col.G / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorB, col.B / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorA, col.A / 255f);
            }
            else if(_event == UiEvent.LeftClickUp)
            {
                sizeX = 200;
                sizeY = 100;
                GenerateMesh();

                Color col = thisFrameData.isHovered ? Color.Wheat : Color.LightSkyBlue;
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorR, col.R / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorG, col.G / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorB, col.B / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorA, col.A / 255f);
            }

            if(_event == UiEvent.HoverStart)
            {
                if (!thisFrameData.isLeftClicked)
                {
                    Color col = Color.Wheat;
                    rendererReference.SetVertexValueAll((int)VertexAttribute.ColorR, col.R / 255f);
                    rendererReference.SetVertexValueAll((int)VertexAttribute.ColorG, col.G / 255f);
                    rendererReference.SetVertexValueAll((int)VertexAttribute.ColorB, col.B / 255f);
                    rendererReference.SetVertexValueAll((int)VertexAttribute.ColorA, col.A / 255f);
                }
            }
            else if(_event == UiEvent.HoverEnd)
            {
                if (!thisFrameData.isLeftClicked)
                {
                    Color col = Color.LightSkyBlue;
                    rendererReference.SetVertexValueAll((int)VertexAttribute.ColorR, col.R / 255f);
                    rendererReference.SetVertexValueAll((int)VertexAttribute.ColorG, col.G / 255f);
                    rendererReference.SetVertexValueAll((int)VertexAttribute.ColorB, col.B / 255f);
                    rendererReference.SetVertexValueAll((int)VertexAttribute.ColorA, col.A / 255f);
                }
            }
        }
    }
}