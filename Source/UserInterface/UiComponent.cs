using System.Numerics;
using Electron2D.Rendering;
using System.Drawing;
using Electron2D.Rendering.Shaders;
using Electron2D.ECS;

namespace Electron2D.UserInterface
{
    public abstract class UiComponent : Entity, IRenderable
    {
        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                InvokeUiAction(UiEvent.Visibility);
            }
        }
        private bool visible = true;
        private bool ignorePostProcessing { get; }
        public bool UsingMeshRenderer { get; }
        public bool UseScreenPosition
        {
            get => useScreenPosition;
            set
            {
                useScreenPosition = value;
                if (meshRenderer != null) meshRenderer.UseUnscaledProjectionMatrix = value;
            }
        }
        private bool useScreenPosition;
        private bool registerRenderable;
        public float SizeX
        {
            get { return sizeX; }
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
        public LayoutGroup ChildLayoutGroup { get; private set; }
        public UiCanvas.UiFrameTickData ThisFrameData = new UiCanvas.UiFrameTickData();
        public UiCanvas.UiFrameTickData LastFrameData = new UiCanvas.UiFrameTickData();
        public UiConstraints Constraints;

        protected bool isLoaded = false;
        protected MeshRenderer meshRenderer;
        private UiCanvas ParentCanvas;

        public bool Interactable = true;
        public float ExtraInteractionPixels;
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

        public bool ShowBoundsDebug
        {
            get => showBoundsDebug;
            set
            {
                showBoundsDebug = value;

                // Creating the sprites for the bounds.
                if (s1 == null && showBoundsDebug)
                {
                    // Initializing debug sprites
                    Shader shader = GlobalShaders.DefaultTexture;
                    s1 = new Sprite(Material.Create(shader, Color.Black), _renderLayer: (int)RenderLayer.Interface + 100);
                    s1.Renderer.UseUnscaledProjectionMatrix = true;
                    s2 = new Sprite(Material.Create(shader, Color.Black), _renderLayer: (int)RenderLayer.Interface + 100);
                    s2.Renderer.UseUnscaledProjectionMatrix = true;
                    s3 = new Sprite(Material.Create(shader, Color.Black), _renderLayer: (int)RenderLayer.Interface + 100);
                    s3.Renderer.UseUnscaledProjectionMatrix = true;
                    s4 = new Sprite(Material.Create(shader, Color.Black), _renderLayer: (int)RenderLayer.Interface + 100);
                    s4.Renderer.UseUnscaledProjectionMatrix = true;
                    s1.Transform.Scale = Vector2.Zero;
                    s2.Transform.Scale = Vector2.Zero;
                    s3.Transform.Scale = Vector2.Zero;
                    s4.Transform.Scale = Vector2.Zero;
                }
            }
        }
        private bool showBoundsDebug = false;
        private Sprite s1, s2, s3, s4;

        public UiComponent(bool _ignorePostProcessing, int _uiRenderLayer = 0, int _sizeX = 100, int _sizeY = 100, int _extraInteractionSize = 0,
            bool _initialize = true, bool _useScreenPosition = true, bool _useMeshRenderer = true, bool _autoRender = true)
        {
            Transform = new Transform();
            Transform.OnPositionChanged += () => InvokeUiAction(UiEvent.Position);

            AddComponent(Transform);
            SizeX = _sizeX;
            SizeY = _sizeY;
            Constraints = new UiConstraints(this);
            UiRenderLayer = _uiRenderLayer;
            UseScreenPosition = _useScreenPosition;
            UsingMeshRenderer = _useMeshRenderer;
            registerRenderable = _autoRender;
            ExtraInteractionPixels = _extraInteractionSize;
            ignorePostProcessing = _ignorePostProcessing;

            if (UsingMeshRenderer)
            {
                meshRenderer = new MeshRenderer(Transform, Material.Create(GlobalShaders.DefaultInterface));
                AddComponent(meshRenderer);
                if (UseScreenPosition) meshRenderer.UseUnscaledProjectionMatrix = true;
            }

            if (_initialize) Initialize();
            SetColor(Color.White);

            if (_autoRender)
            {
                RenderLayerManager.OrderRenderable(this);
            }
            GlobalUI.MainCanvas.RegisterUiComponent(this);
        }

        public bool ShouldIgnorePostProcessing()
        {
            return ignorePostProcessing;
        }

        ~UiComponent()
        {
            if (registerRenderable) RenderLayerManager.RemoveRenderable(this);
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
            ChildLayoutGroup = _layoutGroup;
            ChildLayoutGroup.SetUiParent(this);
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

            return pos.X >= LeftXBound + Transform.Position.X - ExtraInteractionPixels && pos.X <= RightXBound + Transform.Position.X + ExtraInteractionPixels
                && pos.Y >= BottomYBound + Transform.Position.Y - ExtraInteractionPixels && pos.Y <= TopYBound + Transform.Position.Y + ExtraInteractionPixels;
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
            if (ShowBoundsDebug && s1 != null)
            {
                s1.Transform.Position = new Vector2(Transform.Position.X + LeftXBound, Transform.Position.Y + (SizeY / 2f * -Anchor.Y));
                s1.Transform.Scale = new Vector2(1, SizeY);
                s2.Transform.Position = new Vector2(Transform.Position.X + RightXBound, Transform.Position.Y + (SizeY / 2f * -Anchor.Y));
                s2.Transform.Scale = new Vector2(1, SizeY);
                s3.Transform.Position = new Vector2(Transform.Position.X, Transform.Position.Y + TopYBound);
                s3.Transform.Scale = new Vector2(SizeX, 1);
                s4.Transform.Position = new Vector2(Transform.Position.X, Transform.Position.Y + BottomYBound);
                s4.Transform.Scale = new Vector2(SizeX, 1);
            }

            if (Visible && UsingMeshRenderer)
            {
                meshRenderer.Render();
            }
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
        Position,
        Resize,
        Anchor,
        Visibility,
        InteractabilityEnd
    }
}
