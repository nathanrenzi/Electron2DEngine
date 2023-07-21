using System.Numerics;
using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;

namespace Electron2D.Core.UI
{
    public class UiComponent : IRenderable
    {
        public Transform transform { get; private set; } = new Transform();
        public bool visible = true;
        public float sizeX;
        public float sizeY;
        public Vector2 anchor;
        public IRenderer renderer;
        public VertexRenderer rendererReference { get; private set; }
        public int uiRenderLayer { get; private set; }

        public List<UiListener> listeners { get; private set; } = new List<UiListener>();

        public float LeftXBound
        {
            get
            {
                return sizeX + (-anchor.X * sizeX);
            }
        }
        public float RightXBound
        {
            get
            {
                return -sizeX + (-anchor.X * sizeX);
            }
        }
        public float TopYBound
        {
            get
            {
                return -sizeY + (-anchor.Y * sizeY);
            }
        }
        public float BottomYBound
        {
            get
            {
                return sizeY + (-anchor.Y * sizeY);
            }
        }

        public UiComponent(int _uiRenderLayer = 0, IRenderer _customRenderer = null)
        {
            uiRenderLayer = _uiRenderLayer;

            if (_customRenderer != null)
            {
                renderer = _customRenderer;
            }
            else
            {
                // Must implement seperate UI Shader and Renderer, ex. for rounded corners
                rendererReference = new VertexRenderer(transform, new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultVertex.glsl")));
                renderer = rendererReference;
            }

            RenderLayerManager.OrderRenderable(this);
        }

        public void GenerateUiMesh()
        {
            rendererReference.AddVertex(transform.position + new Vector2(LeftXBound, BottomYBound), Color.LightSkyBlue);  // b-left
            rendererReference.AddVertex(transform.position + new Vector2(LeftXBound, TopYBound), Color.LightSkyBlue);     // t-left
            rendererReference.AddVertex(transform.position + new Vector2(RightXBound, TopYBound), Color.LightSkyBlue);    // t-right
            rendererReference.AddVertex(transform.position + new Vector2(RightXBound, BottomYBound), Color.LightSkyBlue); // b-right
           
            rendererReference.AddTriangle(3, 1, 0, 0);
            rendererReference.AddTriangle(3, 2, 1, 0);

            rendererReference.FinalizeVertices();
            rendererReference.Load();
        }

        ~UiComponent()
        {
            RenderLayerManager.RemoveRenderable(this);
        }

        public void SetRenderLayer(int _uiRenderLayer)
        {
            if (_uiRenderLayer == uiRenderLayer) return;
            RenderLayerManager.OrderRenderable(this, true, uiRenderLayer + (int)RenderLayer.Interface, _uiRenderLayer + (int)RenderLayer.Interface);
            uiRenderLayer = _uiRenderLayer;
        }

        public virtual bool CheckBounds(Vector2 _position)
        {
            Vector2 pos = _position - transform.position;

            if (pos.X >= LeftXBound && pos.X <= RightXBound
                && pos.Y >= BottomYBound && pos.Y <= TopYBound)
            {
                // The point is within the bounds
                return true;
            }

            return false;
        }

        public void AddUiListener(UiListener _listener)
        {
            listeners.Add(_listener);
        }

        public void RemoveUiListener(UiListener _listener)
        {
            listeners.Remove(_listener);
        }

        public void InvokeUiAction(UiEvent _event)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnUiAction(this, _event);
            }
        }

        public virtual void Render()
        {
            renderer.Render();
        }

        public int GetRenderLayer() => uiRenderLayer + (int)RenderLayer.Interface;
    }

    public interface UiListener
    {
        public void OnUiAction(object _sender, UiEvent _event);
    }

    public enum UiEvent
    {
        Click,
        ClickDown,
        ClickUp,
        LeftClick,
        LeftClickDown,
        LeftClickUp,
        RightClick,
        RightClickDown,
        RightClickUp,
        Hover,
        HoverStart,
        HoverEnd
    }
}
