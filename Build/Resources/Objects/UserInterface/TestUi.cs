using Electron2D.Core.Rendering;
using Electron2D.Core.UI;
using System.Drawing;

namespace Electron2D.Build.Resources.Objects
{
    public class TestUi : UiComponent
    {
        protected override void OnUiEvent(UiEvent _event)
        {
            if(_event == UiEvent.ClickDown)
            {
                sizeX = 210;
                sizeY = 110;
                transform.position.X += 5;
                GenerateUiMesh();

                Color col = Color.Sienna;
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorR, col.R / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorG, col.G / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorB, col.B / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorA, col.A / 255f);
                return;
            }
            else if(_event == UiEvent.ClickUp)
            {
                sizeX = 200;
                sizeY = 100;
                transform.position.X -= 5;
                GenerateUiMesh();

                Color col = Color.Wheat;
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorR, col.R / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorG, col.G / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorB, col.B / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorA, col.A / 255f);
                return;
            }

            if(_event == UiEvent.HoverStart)
            {
                Color col = Color.Wheat;
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorR, col.R / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorG, col.G / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorB, col.B / 255f);
                rendererReference.SetVertexValueAll((int)VertexAttribute.ColorA, col.A / 255f);
            }
            else if(_event == UiEvent.HoverEnd)
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