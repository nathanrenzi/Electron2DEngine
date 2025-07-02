using System.Numerics;
using Electron2D.Rendering;
using System.Drawing;
using Electron2D.Rendering.Shaders;
using GLFW;

namespace Electron2D.UserInterface
{
    public abstract class UIComponent : IRenderable
    {
        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                InvokeUIEvent(UIEvent.Visibility);
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
                InvokeUIEvent(UIEvent.Resize);
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
                InvokeUIEvent(UIEvent.Resize);
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
                InvokeUIEvent(UIEvent.Anchor);
                UpdateMesh();
            }
        }
        private Vector2 _anchor;

        public event Action OnLoad;
        public event Action OnClick;
        public event Action OnClickDown;
        public event Action OnClickUp;
        public event Action OnLeftClick;
        public event Action OnLeftClickDown;
        public event Action OnLeftClickUp;
        public event Action OnMiddleClick;
        public event Action OnMiddleClickDown;
        public event Action OnMiddleClickUp;
        public event Action OnRightClick;
        public event Action OnRightClickDown;
        public event Action OnRightClickUp;
        public event Action OnHover;
        public event Action OnHoverStart;
        public event Action OnHoverEnd;
        public event Action OnPositionChanged;
        public event Action OnResized;
        public event Action OnAnchorChanged;
        public event Action OnVisibilityChanged;
        public event Action OnInteractabilityEnd;
        public event Action OnFocused;
        public event Action OnFocusLost;
        public bool Focused { get; private set; }

        public MeshRenderer Renderer { get; private set; }
        public Transform Transform { get; private set; }
        public int UIRenderLayer { get; private set; }
        public bool Interactable { get; set; } = true;
        public float ExtraInteractionPixels { get; set; }
        public List<UIListener> Listeners { get; private set; } = new List<UIListener>();
        public LayoutGroup ChildLayoutGroup { get; private set; }
        public UICanvas.UIFrameTickData ThisFrameData = new UICanvas.UIFrameTickData();
        public UICanvas.UIFrameTickData LastFrameData = new UICanvas.UIFrameTickData();
        public UIConstraints Constraints;
        private CursorType _hoverCursorType = CursorType.Arrow;
        protected bool _isLoaded = false;
        private List<UIComponent> _eventSources = new List<UIComponent>();
        private EventSourceListener _eventSourceListener;
        private UICanvas _parentCanvas;
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

        public UIComponent(bool ignorePostProcessing, int uiRenderLayer = 0, int sizeX = 100, int sizeY = 100, int extraInteractionSize = 0,
            bool initialize = true, bool useScreenPosition = true, bool useMeshRenderer = true, bool autoRender = true)
        {
            Transform = new Transform();
            Transform.OnPositionChanged += () => InvokeUIEvent(UIEvent.Position);

            SizeX = sizeX;
            SizeY = sizeY;
            Constraints = new UIConstraints(this);
            UIRenderLayer = uiRenderLayer;
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
            _eventSourceListener = new EventSourceListener();
            _eventSourceListener.OnEvent += InvokeUIEvent;
            UI.MainCanvas.RegisterUiComponent(this);
        }

        public bool ShouldIgnorePostProcessing()
        {
            return _ignorePostProcessing;
        }

        ~UIComponent()
        {
            if (_registerRenderable) RenderLayerManager.RemoveRenderable(this);
            for (int i = 0; i < _eventSources.Count; i++)
            {
                _eventSources[i].RemoveUIListener(_eventSourceListener);
            }
            UI.MainCanvas.UnregisterUiComponent(this);
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

        public void Focus(bool triggerFocusEvent = true)
        {
            if(triggerFocusEvent) _parentCanvas.Focus(this);
            Focused = true;
        }

        public void Unfocus(bool triggerFocusEvent = true)
        {
            if (triggerFocusEvent) _parentCanvas.Unfocus(this);
            Focused = false;
        }

        public void SetHoverCursorType(CursorType type)
        {
            _hoverCursorType = type;
        }

        protected virtual void Load()
        {
            if (UsingMeshRenderer) Renderer.Load();
            InvokeUIEvent(UIEvent.Load);
        }

        protected void AddEventSource(UIComponent component)
        {
            if(!_eventSources.Contains(component))
            {
                _eventSources.Add(component);
                component.AddUIListener(_eventSourceListener);
            }
        }

        protected void RemoveEventSource(UIComponent component)
        {
            if(_eventSources.Contains(component))
            {
                _eventSources.Remove(component);
                component.RemoveUIListener(_eventSourceListener);
            }
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
            if (uiRenderLayer == UIRenderLayer) return;
            RenderLayerManager.OrderRenderable(this, true, UIRenderLayer + (int)RenderLayer.Interface, uiRenderLayer + (int)RenderLayer.Interface);
            UIRenderLayer = uiRenderLayer;
        }

        public virtual bool CheckBounds(Vector2 position)
        {
            Vector2 pos = position;

            return pos.X >= LeftXBound + Transform.Position.X - ExtraInteractionPixels && pos.X <= RightXBound + Transform.Position.X + ExtraInteractionPixels
                && pos.Y >= BottomYBound + Transform.Position.Y - ExtraInteractionPixels && pos.Y <= TopYBound + Transform.Position.Y + ExtraInteractionPixels;
        }

        public void AddUIListener(UIListener listener)
        {
            if (Listeners.Contains(listener))
            {
                Debug.LogError("UI LISTENER: Trying to add a listener that is already registered.");
                return;
            }
            Listeners.Add(listener);
        }

        public void RemoveUIListener(UIListener listener)
        {
            Listeners.Remove(listener);
        }

        public void InvokeUIEvent(UIEvent uiEvent)
        {
            OnUIEvent(uiEvent);

            for (int i = 0; i < Listeners.Count; i++)
            {
                Listeners[i].OnUiAction(this, uiEvent);
            }

            switch (uiEvent)
            {
                case UIEvent.Load: OnLoad?.Invoke();break;
                case UIEvent.Click: OnClick?.Invoke(); break;
                case UIEvent.ClickDown: OnClickDown?.Invoke(); break;
                case UIEvent.ClickUp: OnClickUp?.Invoke(); break;
                case UIEvent.LeftClick: OnLeftClick?.Invoke(); break;
                case UIEvent.LeftClickDown: OnLeftClickDown?.Invoke(); break;
                case UIEvent.LeftClickUp: OnLeftClickUp?.Invoke(); break;
                case UIEvent.MiddleClick: OnMiddleClick?.Invoke(); break;
                case UIEvent.MiddleClickDown: OnMiddleClickDown?.Invoke(); break;
                case UIEvent.MiddleClickUp: OnMiddleClickUp?.Invoke(); break;
                case UIEvent.RightClick: OnRightClick?.Invoke(); break;
                case UIEvent.RightClickDown: OnRightClickDown?.Invoke(); break;
                case UIEvent.RightClickUp: OnRightClickUp?.Invoke(); break;
                case UIEvent.Hover: OnHover?.Invoke(); break;
                case UIEvent.HoverStart:
                    if(_hoverCursorType != CursorType.Arrow) Cursor.SetType(_hoverCursorType);
                    OnHoverStart?.Invoke();
                    break;
                case UIEvent.HoverEnd:
                    if (_hoverCursorType != CursorType.Arrow) Cursor.SetType(CursorType.Arrow);
                    OnHoverEnd?.Invoke();
                    break;
                case UIEvent.Position: OnPositionChanged?.Invoke(); break;
                case UIEvent.Resize: OnResized?.Invoke(); break;
                case UIEvent.Anchor: OnAnchorChanged?.Invoke(); break;
                case UIEvent.Visibility: OnVisibilityChanged?.Invoke(); break;
                case UIEvent.InteractabilityEnd: OnInteractabilityEnd?.Invoke(); break;
                case UIEvent.Focus: OnFocused?.Invoke(); break;
                case UIEvent.LoseFocus: OnFocusLost?.Invoke(); break;
            }
        }

        public void SetParentCanvas(UICanvas canvas)
        {
            _parentCanvas = canvas;
        }

        public UICanvas GetParentCanvas()
        {
            return _parentCanvas;
        }

        protected virtual void OnUIEvent(UIEvent uiEvent) { }

        public virtual void Render()
        {
            if (Visible && UsingMeshRenderer)
            {
                Renderer.Render();
            }
        }

        public int GetRenderLayer() => UIRenderLayer + (int)RenderLayer.Interface;

        private class EventSourceListener : UIListener
        {
            public event Action<UIEvent> OnEvent;

            public void OnUiAction(object sender, UIEvent uiEvent)
            {
                OnEvent?.Invoke(uiEvent);
            }
        }
    }

    public interface UIListener
    {
        public void OnUiAction(object sender, UIEvent uiEvent);
    }

    public enum UIEvent
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
        InteractabilityEnd,
        Focus,
        LoseFocus
    }
}
