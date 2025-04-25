using System.Numerics;
using Electron2D.Rendering;
using System.Drawing;
using Electron2D.Rendering.Shaders;

namespace Electron2D.UserInterface
{
    public abstract class UiComponent : IRenderable
    {
        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                InvokeUiAction(UiEvent.Visibility);
            }
        }
        private bool _visible = true;
        private bool _ignorePostProcessing { get; }
        public bool UsingMeshRenderer { get; }
        public bool UseScreenPosition
        {
            get => _useScreenPosition;
            set
            {
                _useScreenPosition = value;
                if (Renderer != null) Renderer.UseUnscaledProjectionMatrix = value;
            }
        }
        private bool _useScreenPosition;
        private bool _registerRenderable;
        public float SizeX
        {
            get { return _sizeX; }
            set
            {
                _sizeX = value;
                InvokeUiAction(UiEvent.Resize);
                UpdateMesh();
            }
        }
        private float _sizeX;
        public float SizeY
        {
            get { return _sizeY; }
            set
            {
                _sizeY = value;
                InvokeUiAction(UiEvent.Resize);
                UpdateMesh();
            }
        }
        private float _sizeY;
        public Vector2 Anchor
        {
            get => _anchor;
            set
            {
                _anchor = value;
                InvokeUiAction(UiEvent.Anchor);
                UpdateMesh();
            }
        }
        private Vector2 _anchor;

        public MeshRenderer Renderer { get; private set; }
        public Transform Transform { get; private set; }
        public int UiRenderLayer { get; private set; }
        public bool Interactable { get; set; } = true;
        public float ExtraInteractionPixels { get; set; }
        public List<UiListener> Listeners { get; private set; } = new List<UiListener>();
        public LayoutGroup ChildLayoutGroup { get; private set; }
        public UiCanvas.UiFrameTickData ThisFrameData = new UiCanvas.UiFrameTickData();
        public UiCanvas.UiFrameTickData LastFrameData = new UiCanvas.UiFrameTickData();
        public UiConstraints Constraints;

        protected bool _isLoaded = false;
        private UiCanvas _parentCanvas;
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

        public UiComponent(bool ignorePostProcessing, int uiRenderLayer = 0, int sizeX = 100, int sizeY = 100, int extraInteractionSize = 0,
            bool initialize = true, bool useScreenPosition = true, bool useMeshRenderer = true, bool autoRender = true)
        {
            Transform = new Transform();
            Transform.OnPositionChanged += () => InvokeUiAction(UiEvent.Position);

            SizeX = sizeX;
            SizeY = sizeY;
            Constraints = new UiConstraints(this);
            UiRenderLayer = uiRenderLayer;
            UseScreenPosition = useScreenPosition;
            UsingMeshRenderer = useMeshRenderer;
            _registerRenderable = autoRender;
            ExtraInteractionPixels = extraInteractionSize;
            _ignorePostProcessing = ignorePostProcessing;

            if (UsingMeshRenderer)
            {
                Renderer = new MeshRenderer(Transform, Material.Create(GlobalShaders.DefaultInterface));
                if (UseScreenPosition) Renderer.UseUnscaledProjectionMatrix = true;
            }

            if (initialize) Initialize();
            SetColor(Color.White);

            if (autoRender)
            {
                RenderLayerManager.OrderRenderable(this);
            }
            GlobalUI.MainCanvas.RegisterUiComponent(this);
        }

        public bool ShouldIgnorePostProcessing()
        {
            return _ignorePostProcessing;
        }

        ~UiComponent()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_registerRenderable) RenderLayerManager.RemoveRenderable(this);
            GlobalUI.MainCanvas.UnregisterUiComponent(this);
            GC.SuppressFinalize(this);
        }

        public void Initialize()
        {
            ApplyConstraints();
            if (_isLoaded == false)
            {
                Load();
                _isLoaded = true;
            }
        }

        protected virtual void Load()
        {
            if (UsingMeshRenderer) Renderer.Load();
            InvokeUiAction(UiEvent.Load);
        }

        public virtual void UpdateMesh() { }

        public virtual void SetColor(Color color)
        {
            if (UsingMeshRenderer) Renderer.Material.MainColor = color;
        }

        public void SetLayoutGroup(LayoutGroup layoutGroup)
        {
            ChildLayoutGroup = layoutGroup;
            ChildLayoutGroup.SetUiParent(this);
        }

        protected virtual void ApplyConstraints()
        {
            Constraints.ApplyConstraints();
        }

        public void SetRenderLayer(int uiRenderLayer)
        {
            if (uiRenderLayer == UiRenderLayer) return;
            RenderLayerManager.OrderRenderable(this, true, UiRenderLayer + (int)RenderLayer.Interface, uiRenderLayer + (int)RenderLayer.Interface);
            UiRenderLayer = uiRenderLayer;
        }

        public virtual bool CheckBounds(Vector2 position)
        {
            Vector2 pos = position;

            return pos.X >= LeftXBound + Transform.Position.X - ExtraInteractionPixels && pos.X <= RightXBound + Transform.Position.X + ExtraInteractionPixels
                && pos.Y >= BottomYBound + Transform.Position.Y - ExtraInteractionPixels && pos.Y <= TopYBound + Transform.Position.Y + ExtraInteractionPixels;
        }

        public void AddUiListener(UiListener listener)
        {
            if (Listeners.Contains(listener))
            {
                Debug.LogError("UI LISTENER: Trying to add a listener that is already registered.");
                return;
            }
            Listeners.Add(listener);
        }

        public void RemoveUiListener(UiListener listener)
        {
            Listeners.Remove(listener);
        }

        public void InvokeUiAction(UiEvent uiEvent)
        {
            for (int i = 0; i < Listeners.Count; i++)
            {
                Listeners[i].OnUiAction(this, uiEvent);
            }
            OnUiEvent(uiEvent);
        }

        public void SetParentCanvas(UiCanvas canvas)
        {
            _parentCanvas = canvas;
        }

        public UiCanvas GetParentCanvas()
        {
            return _parentCanvas;
        }

        protected virtual void OnUiEvent(UiEvent uiEvent) { }

        public virtual void Render()
        {
            if (Visible && UsingMeshRenderer)
            {
                Renderer.Render();
            }
        }

        public int GetRenderLayer() => UiRenderLayer + (int)RenderLayer.Interface;
    }

    public interface UiListener
    {
        public void OnUiAction(object sender, UiEvent uiEvent);
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
        Position,
        Resize,
        Anchor,
        Visibility,
        InteractabilityEnd
    }
}
