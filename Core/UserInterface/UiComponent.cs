using System.Numerics;
using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;
using Electron2D.Core.UserInterface;

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

        private bool isLoaded = false;

        public float RightXBound
        {
            get
            {
                return sizeX + (-anchor.X * sizeX);
            }
        }
        public float LeftXBound
        {
            get
            {
                return -sizeX + (-anchor.X * sizeX);
            }
        }
        public float BottomYBound
        {
            get
            {
                return -sizeY + (-anchor.Y * sizeY);
            }
        }
        public float TopYBound
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
            UiMaster.display.RegisterUiComponent(this);
        }

        public void GenerateUiMesh()
        {
            rendererReference.ClearTempLists();
            rendererReference.AddVertex(transform.position + new Vector2(LeftXBound, BottomYBound), Color.LightSkyBlue);  // b-left
            rendererReference.AddVertex(transform.position + new Vector2(LeftXBound, TopYBound), Color.LightSkyBlue);     // t-left
            rendererReference.AddVertex(transform.position + new Vector2(RightXBound, TopYBound), Color.LightSkyBlue);    // t-right
            rendererReference.AddVertex(transform.position + new Vector2(RightXBound, BottomYBound), Color.LightSkyBlue); // b-right
           
            rendererReference.AddTriangle(3, 1, 0, 0);
            rendererReference.AddTriangle(3, 2, 1, 0);

            rendererReference.FinalizeVertices();

            if(isLoaded == false) rendererReference.Load();
            isLoaded = true;
        }

        ~UiComponent()
        {
            RenderLayerManager.RemoveRenderable(this);
            UiMaster.display.UnregisterUiComponent(this);
        }

        public void SetRenderLayer(int _uiRenderLayer)
        {
            if (_uiRenderLayer == uiRenderLayer) return;
            RenderLayerManager.OrderRenderable(this, true, uiRenderLayer + (int)RenderLayer.Interface, _uiRenderLayer + (int)RenderLayer.Interface);
            uiRenderLayer = _uiRenderLayer;
        }

        public virtual bool CheckBounds(Vector2 _position)
        {
            Vector2 pos = _position;

            return pos.X >= LeftXBound + transform.position.X && pos.X <= RightXBound + transform.position.X
                && pos.Y >= BottomYBound + transform.position.Y && pos.Y <= TopYBound + transform.position.Y;
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

            OnUiEvent(_event);
        }

        protected virtual void OnUiEvent(UiEvent _event) { }

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
        MiddleClick,
        MiddleClickDown,
        MiddleClickUp,
        RightClick,
        RightClickDown,
        RightClickUp,
        Hover,
        HoverStart,
        HoverEnd
    }
}
