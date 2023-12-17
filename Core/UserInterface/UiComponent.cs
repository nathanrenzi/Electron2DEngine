using System.Numerics;
using Electron2D.Core.Rendering;
using System.Drawing;
using Electron2D.Core.UserInterface;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.ECS;

namespace Electron2D.Core.UI
{
    public abstract class UiComponent : Entity, IRenderable
    {
        public bool Visible = true;

        public bool UsingMeshRenderer { get; }
        public bool UseScreenPosition
        {
            get => useScreenPosition;
            set
            {
                useScreenPosition = value;
                if(meshRenderer != null) meshRenderer.UseUnscaledProjectionMatrix = value;
            }
        }
        private bool useScreenPosition;
        public float SizeX
        {
            get{ return sizeX; }
            set
            {
                sizeX = value;
                InvokeUiAction(UiEvent.Resize);
                UpdateMesh();
            }
        }
        private float sizeX;
        public float SizeY
        {
            get { return sizeY; }
            set
            {
                sizeY = value;
                InvokeUiAction(UiEvent.Resize);
                UpdateMesh();
            }
        }
        private float sizeY;
        public Vector2 Anchor
        {
            get => anchor;
            set
            {
                anchor = value;
                InvokeUiAction(UiEvent.Anchor);
                UpdateMesh();
            }
        }
        private Vector2 anchor;


        public Transform Transform;
        public int UiRenderLayer { get; private set; }
        public List<UiListener> Listeners { get; private set; } = new List<UiListener>();
        public LayoutGroup Layout { get; private set; }
        public UiCanvas.UiFrameTickData ThisFrameData = new UiCanvas.UiFrameTickData();
        public UiCanvas.UiFrameTickData LastFrameData = new UiCanvas.UiFrameTickData();
        public UiConstraints Constraints;

        protected bool isLoaded = false;
        protected MeshRenderer meshRenderer;
        private UiCanvas ParentCanvas;

        public float RightXBound
        {
            get
            {
                return SizeX / 2f + (-Anchor.X * SizeX / 2f);
            }
        }
        public float LeftXBound
        {
            get
            {
                return -SizeX / 2f + (-Anchor.X * SizeX / 2f);
            }
        }
        public float BottomYBound
        {
            get
            {
                return -SizeY / 2f + (-Anchor.Y * SizeY / 2f);
            }
        }
        public float TopYBound
        {
            get
            {
                return SizeY / 2f + (-Anchor.Y * SizeY / 2f);
            }
        }

        public UiComponent(int _uiRenderLayer = 0, int _sizeX = 100, int _sizeY = 100, bool _initialize = true, bool _useScreenPosition = true, bool _useMeshRenderer = true)
        {
            Transform = new Transform();
            AddComponent(Transform);
            SizeX = _sizeX;
            SizeY = _sizeY;
            Constraints = new UiConstraints(this);
            UiRenderLayer = _uiRenderLayer;
            UseScreenPosition = _useScreenPosition;
            UsingMeshRenderer = _useMeshRenderer;

            if(UsingMeshRenderer)
            {
                meshRenderer = new MeshRenderer(Transform, Material.Create(GlobalShaders.DefaultTexture));
                AddComponent(meshRenderer);
                if (UseScreenPosition) meshRenderer.UseUnscaledProjectionMatrix = true;
            }

            if (_initialize) Initialize();
            SetColor(Color.White);

            RenderLayerManager.OrderRenderable(this);
            GlobalUI.MainCanvas.RegisterUiComponent(this);
        }

        ~UiComponent()
        {
            RenderLayerManager.RemoveRenderable(this);
            GlobalUI.MainCanvas.UnregisterUiComponent(this);
        }

        public void Initialize()
        {
            ApplyConstraints();
            if (isLoaded == false)
            {
                Load();
                isLoaded = true;
            }
        }

        protected virtual void Load()
        {
            if (UsingMeshRenderer) meshRenderer.Load();
            InvokeUiAction(UiEvent.Load);
        }

        public virtual void UpdateMesh() { }

        public virtual void SetColor(Color _color)
        {
            if (UsingMeshRenderer) meshRenderer.Material.MainColor = _color;
        }

        public void SetLayoutGroup(LayoutGroup _layoutGroup)
        {
            Layout = _layoutGroup;
            Layout.SetUiParent(this);
        }

        protected virtual void ApplyConstraints()
        {
            Constraints.ApplyConstraints();
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
            if (Listeners.Contains(_listener))
            {
                Debug.LogError("UI LISTENER: Trying to add a listener that is already registered.");
                return;
            }
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

        public void SetParentCanvas(UiCanvas _canvas)
        {
            ParentCanvas = _canvas;
        }

        public UiCanvas GetParentCanvas()
        {
            return ParentCanvas;
        }

        protected virtual void OnUiEvent(UiEvent _event) { }

        public virtual void Render()
        {
            if(Visible && UsingMeshRenderer) meshRenderer.Render();
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
        HoverEnd,
        Resize,
        Anchor,
    }
}
