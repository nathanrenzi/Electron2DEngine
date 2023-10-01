using System.Numerics;
using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering;
using System.Drawing;
using Electron2D.Core.UserInterface;
using Electron2D.Core.Rendering.Shaders;

namespace Electron2D.Core.UI
{
    public class UiComponent : IRenderable
    {
        public Transform transform { get; private set; } = new Transform();
        public bool visible = true;
        public bool useScreenPosition = true;
        public float sizeX;
        public float sizeY;
        public Vector2 anchor;
        public MeshRenderer renderer { get; private set; }
        public int uiRenderLayer { get; private set; }
        public List<UiListener> listeners { get; private set; } = new List<UiListener>();

        public UiFrameTickData thisFrameData = new UiFrameTickData();
        public UiFrameTickData lastFrameData = new UiFrameTickData();
        public UiConstraints constraints;

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

        public UiComponent(int _uiRenderLayer = 0, int _sizeX = 100, int _sizeY = 100, bool _initialize = true)
        {
            constraints = new UiConstraints(this);
            uiRenderLayer = _uiRenderLayer;

            renderer = new MeshRenderer(transform, Material.Create(GlobalShaders.DefaultTexture));
            renderer.UseUnscaledProjectionMatrix = true;

            sizeX = _sizeX;
            sizeY = _sizeY;
            if (_initialize) Initialize();
            SetColor(Color.White); // Setting the default color

            RenderLayerManager.OrderRenderable(this);
            UiMaster.display.RegisterUiComponent(this);
        }

        public void Initialize()
        {
            ApplyConstraints();

            if (isLoaded == false)
            {
                renderer.Load();
                InvokeUiAction(UiEvent.Load);
                isLoaded = true;
            }
        }

        public void SetColor(Color _color)
        {
            Material startMaterial = renderer.GetMaterial();
            renderer.SetMaterial(Material.Create(startMaterial.Shader, startMaterial.MainTexture, _color, startMaterial.UseLinearFiltering));
        }

        protected virtual void ApplyConstraints()
        {
            constraints.ApplyConstraints();
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
            if (!listeners.Contains(_listener)) return;
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
            if(visible) renderer.Render();
        }

        public int GetRenderLayer() => uiRenderLayer + (int)RenderLayer.Interface;
    }

    public interface UiListener
    {
        public void OnUiAction(object _sender, UiEvent _event);
    }

    public enum UiEvent
    {
        Load,
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
