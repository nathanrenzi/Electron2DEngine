using System.Numerics;
using Electron2D.Core.Rendering;
using System.Drawing;
using Electron2D.Core.UserInterface;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.ECS;

namespace Electron2D.Core.UI
{
    public class UiComponent : Entity, IRenderable
    {
        public bool Visible = true;
        public bool UseScreenPosition = true; // Add functionality to make this editable at runtime
        public float SizeX;
        public float SizeY;
        public Vector2 Anchor;
        public Transform Transform;
        public MeshRenderer Renderer { get; private set; }
        public int UiRenderLayer { get; private set; }
        public List<UiListener> Listeners { get; private set; } = new List<UiListener>();

        public UiFrameTickData ThisFrameData = new UiFrameTickData();
        public UiFrameTickData LastFrameData = new UiFrameTickData();
        public UiConstraints Constraints;

        private bool isLoaded = false;

        public float RightXBound
        {
            get
            {
                return SizeX + (-Anchor.X * SizeX);
            }
        }
        public float LeftXBound
        {
            get
            {
                return -SizeX + (-Anchor.X * SizeX);
            }
        }
        public float BottomYBound
        {
            get
            {
                return -SizeY + (-Anchor.Y * SizeY);
            }
        }
        public float TopYBound
        {
            get
            {
                return SizeY + (-Anchor.Y * SizeY);
            }
        }

        public UiComponent(int _uiRenderLayer = 0, int _sizeX = 100, int _sizeY = 100, bool _initialize = true, bool _useScreenPosition = true)
        {
            Transform = new Transform();
            AddComponent(Transform);

            Constraints = new UiConstraints(this);
            UiRenderLayer = _uiRenderLayer;
            UseScreenPosition = _useScreenPosition;

            Renderer = new MeshRenderer(Transform, Material.Create(GlobalShaders.DefaultTexture));
            if(UseScreenPosition) Renderer.UseUnscaledProjectionMatrix = true;

            SizeX = _sizeX;
            SizeY = _sizeY;
            if (_initialize) Initialize();
            SetColor(Color.White); // Setting the default color

            RenderLayerManager.OrderRenderable(this);
            UiMaster.Display.RegisterUiComponent(this);
        }

        public void Initialize()
        {
            ApplyConstraints();

            if (isLoaded == false)
            {
                Renderer.Load();
                InvokeUiAction(UiEvent.Load);
                isLoaded = true;
            }
        }

        public void SetColor(Color _color)
        {
            Material startMaterial = Renderer.GetMaterial();
            Renderer.SetMaterial(Material.Create(startMaterial.Shader, startMaterial.MainTexture, _color, startMaterial.UseLinearFiltering));
        }

        protected virtual void ApplyConstraints()
        {
            Constraints.ApplyConstraints();
        }

        ~UiComponent()
        {
            RenderLayerManager.RemoveRenderable(this);
            UiMaster.Display.UnregisterUiComponent(this);
        }

        public void SetRenderLayer(int _uiRenderLayer)
        {
            if (_uiRenderLayer == UiRenderLayer) return;
            RenderLayerManager.OrderRenderable(this, true, UiRenderLayer + (int)RenderLayer.Interface, _uiRenderLayer + (int)RenderLayer.Interface);
            UiRenderLayer = _uiRenderLayer;
        }

        public virtual bool CheckBounds(Vector2 _position)
        {
            Vector2 pos = _position;

            return pos.X >= LeftXBound + Transform.Position.X && pos.X <= RightXBound + Transform.Position.X
                && pos.Y >= BottomYBound + Transform.Position.Y && pos.Y <= TopYBound + Transform.Position.Y;
        }

        public void AddUiListener(UiListener _listener)
        {
            if (!Listeners.Contains(_listener)) return;
            Listeners.Add(_listener);
        }

        public void RemoveUiListener(UiListener _listener)
        {
            Listeners.Remove(_listener);
        }

        public void InvokeUiAction(UiEvent _event)
        {
            for (int i = 0; i < Listeners.Count; i++)
            {
                Listeners[i].OnUiAction(this, _event);
            }

            OnUiEvent(_event);
        }

        protected virtual void OnUiEvent(UiEvent _event) { }

        public virtual void Render()
        {
            if(Visible) Renderer.Render();
        }

        public int GetRenderLayer() => UiRenderLayer + (int)RenderLayer.Interface;
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
