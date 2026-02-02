using Electron2D.Rendering;
using Electron2D.Rendering.Shaders;
using GLFW;
using System.Drawing;
using System.Numerics;

namespace Electron2D.UI
{
    public abstract class UIElement : IRenderable
    {
        public UIElement Parent { get; private set; }
        private List<UIElement> _children = new List<UIElement>();
        public IReadOnlyList<UIElement> Children => _children;
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    InvalidateArrange();
                }
            }
        }
        private Vector2 _position;
        public Vector2 Size
        {
            get => _size;
            set
            {
                if (_size != value)
                {
                    _size = value;
                    InvalidateMeasure();
                    UpdateMesh();
                }
            }
        }
        private Vector2 _size;
        public Vector2 DesiredSize { get; private set; }
        public Vector2 Pivot
        {
            get => _pivot;
            set
            {
                if(_pivot != value)
                {
                    _pivot = value;
                    InvalidateArrange();
                    UpdateMesh();
                }
            }
        }
        private Vector2 _pivot = Vector2.Zero;
        public Vector2 Anchor
        {
            get => _anchor;
            set
            {
                if (_anchor != value)
                {
                    _anchor = value;
                    InvalidateArrange();
                }
            }
        }
        private Vector2 _anchor = Vector2.Zero;
        public Border Margin { get; set; }
        public Border Padding { get; set; }
        public Vector2 MinSize { get; set; } = Vector2.Zero;
        public Vector2 MaxSize { get; set; } = new Vector2(float.MaxValue, float.MaxValue);
        public float ExtraInteractionPixels { get; set; }
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public bool Interactable { get; set; } = true;
        public bool Focused { get; internal set; }
        public bool IsMeasureValid { get; private set; }
        public bool IsArrangeValid { get; private set; }

        public ILayout Layout { get; set; }
        private List<IConstraint> _constraints = new List<IConstraint>();

        public MeshRenderer Renderer { get; protected set; }
        public int UIRenderLayer { get; private set; }
        public bool UseScreenPosition { get; set; } = true;
        public bool IgnorePostProcessing { get; private set; }
        private bool _useMeshRenderer;
        private CursorType _hoverCursorType = CursorType.Arrow;

        private Dictionary<UIEventType, List<Action<UIEvent>>> _eventHandlers = new Dictionary<UIEventType, List<Action<UIEvent>>>();

        public UIElement(int sizeX, int sizeY, int uiRenderLayer = 0, bool useScreenPosition = true, bool ignorePostProcessing = true, bool useMeshRenderer = true)
        {
            _position = Vector2.Zero;
            _size = new Vector2(sizeX, sizeY);
            DesiredSize = Vector2.Zero;

            UIRenderLayer = uiRenderLayer;
            UseScreenPosition = useScreenPosition;
            IgnorePostProcessing = ignorePostProcessing;
            _useMeshRenderer = useMeshRenderer;

            if (_useMeshRenderer)
            {
                Renderer = new MeshRenderer(Material.Create(GlobalShaders.DefaultInterface));
                Renderer.UseUnscaledProjectionMatrix = UseScreenPosition;
            }

            UICanvas.Instance?.RegisterUIElement(this);
            RenderLayerManager.OrderRenderable(this);
        }

        public void AddChild(UIElement child)
        {
            if (child.Parent != null)
                child.Parent.RemoveChild(child);

            RenderLayerManager.RemoveRenderable(child);
            _children.Add(child);
            child.Parent = this;
            InvalidateMeasure();
        }

        public void RemoveChild(UIElement child)
        {
            if (_children.Remove(child))
            {
                RenderLayerManager.OrderRenderable(child);
                child.Parent = null;
                InvalidateMeasure();
            }
        }

        public void ClearChildren()
        {
            foreach (var child in _children)
            {
                child.Parent = null;
            }
            _children.Clear();
            InvalidateMeasure();
        }

        public void InvalidateMeasure()
        {
            if (!IsMeasureValid) return;

            IsMeasureValid = false;
            IsArrangeValid = false;

            Parent?.InvalidateMeasure();
        }

        public void InvalidateArrange()
        {
            if (!IsArrangeValid) return;
            IsArrangeValid = false;
        }

        /// <summary>
        /// Measure pass. Calculate desired size given available space.
        /// </summary>
        public Vector2 Measure(Vector2 availableSize)
        {
            if (!Visible)
            {
                DesiredSize = Vector2.Zero;
                IsMeasureValid = true;
                return DesiredSize;
            }

            availableSize = new Vector2(
                Math.Max(0, availableSize.X - Margin.Left - Margin.Right),
                Math.Max(0, availableSize.Y - Margin.Top - Margin.Bottom)
            );

            Vector2 desiredSize;
            if (Layout != null)
            {
                desiredSize = Layout.Measure(this, availableSize);
            }
            else
            {
                desiredSize = MeasureCore(availableSize);
            }

            desiredSize = new Vector2(
                Math.Clamp(desiredSize.X, MinSize.X, MaxSize.X),
                Math.Clamp(desiredSize.Y, MinSize.Y, MaxSize.Y)
            );

            DesiredSize = new Vector2(
                desiredSize.X + Margin.Left + Margin.Right,
                desiredSize.Y + Margin.Top + Margin.Bottom
            );

            IsMeasureValid = true;
            return DesiredSize;
        }

        protected virtual Vector2 MeasureCore(Vector2 availableSize)
        {
            availableSize = new Vector2(
                Math.Max(0, availableSize.X - Padding.Left - Padding.Right),
                Math.Max(0, availableSize.Y - Padding.Top - Padding.Bottom)
            );

            Vector2 maxChildSize = Vector2.Zero;
            foreach (var child in _children)
            {
                if (!child.Visible) continue;
                var childDesired = child.Measure(availableSize);
                maxChildSize = Vector2.Max(maxChildSize, childDesired);
            }

            return new Vector2(
                maxChildSize.X + Padding.Left + Padding.Right,
                maxChildSize.Y + Padding.Top + Padding.Bottom
            );
        }

        /// <summary>
        /// Arrange pass. Assign final positions and sizes.
        /// </summary>
        public void Arrange(Rect finalRect)
        {
            if (!Visible)
            {
                IsArrangeValid = true;
                return;
            }

            finalRect = new Rect(
                finalRect.X + Margin.Left,
                finalRect.Y + Margin.Top,
                Math.Max(0, finalRect.Width - Margin.Left - Margin.Right),
                Math.Max(0, finalRect.Height - Margin.Top - Margin.Bottom)
            );

            finalRect.Width = Math.Clamp(finalRect.Width, MinSize.X, MaxSize.X);
            finalRect.Height = Math.Clamp(finalRect.Height, MinSize.Y, MaxSize.Y);

            _size = new Vector2(finalRect.Width, finalRect.Height);

            if(Parent != null)
            {
                _position = new Vector2(finalRect.X, finalRect.Y);
            }

            if (Layout != null)
            {
                Layout.Arrange(this, finalRect);
            }
            else
            {
                ArrangeCore(finalRect);
            }

            foreach (var constraint in _constraints)
            {
                constraint.Apply(this);
            }

            UpdateMesh();

            IsArrangeValid = true;
        }

        protected virtual void ArrangeCore(Rect finalRect)
        {
            Rect childRect = new Rect(
                Padding.Left,
                Padding.Top,
                Math.Max(0, finalRect.Width - Padding.Left - Padding.Right),
                Math.Max(0, finalRect.Height - Padding.Top - Padding.Bottom)
            );

            foreach (var child in _children)
            {
                if (!child.Visible) continue;
                child.Arrange(CalculateAnchoredRect(child, childRect));
            }
        }

        private Rect CalculateAnchoredRect(UIElement child, Rect rect)
        {
            return new Rect(
                rect.X + child.Anchor.X * rect.Width,
                rect.Y + child.Anchor.Y * rect.Height,
                rect.Width,
                rect.Height
            );
        }

        public void AddConstraint(IConstraint constrant)
        {
            _constraints.Add(constrant);
            InvalidateArrange();
        }

        public void RemoveConstraint(IConstraint constraint)
        {
            _constraints.Remove(constraint);
            InvalidateArrange();
        }

        public void ClearConstraints()
        {
            _constraints.Clear(); 
            InvalidateArrange();
        }

        public void AddEventListener(UIEventType type, Action<UIEvent> handler)
        {
            if (!_eventHandlers.ContainsKey(type))
                _eventHandlers[type] = new List<Action<UIEvent>>();

            _eventHandlers[type].Add(handler);
        }

        public void RemoveEventListener(UIEventType type, Action<UIEvent> handler)
        {
            if (_eventHandlers.ContainsKey(type))
                _eventHandlers[type].Remove(handler);
        }

        public void RaiseEvent(UIEvent evt)
        {
            evt.Current = this;

            if (_eventHandlers.TryGetValue(evt.Type, out var handlers))
            {
                foreach (var handler in handlers.ToArray())
                {
                    handler(evt);
                    if (evt.IsPropagationStopped) return;
                }
            }

            HandleBuiltInEvent(evt);

            if (evt.Phase == EventPhase.Bubble && Parent != null && !evt.IsPropagationStopped)
            {
                Parent.RaiseEvent(evt);
            }
        }

        private void HandleBuiltInEvent(UIEvent evt)
        {
            switch (evt.Type)
            {
                case UIEventType.MouseEnter:
                    if (_hoverCursorType != CursorType.Arrow)
                        Cursor.SetType(_hoverCursorType);
                    break;

                case UIEventType.MouseLeave:
                    if (_hoverCursorType != CursorType.Arrow)
                        Cursor.SetType(CursorType.Arrow);
                    break;
            }
        }

        public Rect GetVirtualRect()
        {
            Vector2 pos = GetVirtualPosition();

            Vector2 pivotOffset = new Vector2(
                -Pivot.X * Size.X,
                -Pivot.Y * Size.Y
            );

            return new Rect(
                pos.X + pivotOffset.X - ExtraInteractionPixels,
                pos.Y + pivotOffset.Y - ExtraInteractionPixels,
                Size.X + ExtraInteractionPixels * 2,
                Size.Y + ExtraInteractionPixels * 2
            );
        }

        public Vector2 GetVirtualPosition()
        {
            if (Parent == null)
            {
                return Position;
            }

            Vector2 parentVirtualPos = Parent.GetVirtualPosition();
            Vector2 parentPivotOffset = new Vector2(
                -Parent.Pivot.X * Parent.Size.X,
                -Parent.Pivot.Y * Parent.Size.Y
            );
            Vector2 parentTopLeft = parentVirtualPos + parentPivotOffset;

            return parentTopLeft + Position;
        }

        public bool HitTest(Vector2 virtualPoint)
        {
            if (!Visible || !Enabled || !Interactable)
                return false;

            Rect rect = GetVirtualRect();
            return virtualPoint.X >= rect.X &&
                   virtualPoint.X <= rect.X + rect.Width &&
                   virtualPoint.Y >= rect.Y &&
                   virtualPoint.Y <= rect.Y + rect.Height;
        }

        public abstract void UpdateMesh();

        public virtual void SetColor(Color color)
        {
            if (Renderer != null)
            {
                Renderer.Material.MainColor = color;
            }
        }

        public void SetHoverCursorType(CursorType type)
        {
            _hoverCursorType = type;
        }

        public virtual void Render()
        {
            if (!Visible) return;

            if (Renderer != null)
            {
                Vector2 pos = UICanvas.Instance.VirtualToScreen(GetVirtualPosition());
                Renderer.GetMaterial().Shader.SetMatrix4x4("model", Matrix4x4.CreateTranslation(pos.X, pos.Y, 0));
                Renderer.GetMaterial().Shader.SetMatrix4x4("uiMatrix",
                    UseScreenPosition ? UICanvas.Instance.UIModelMatrix : Matrix4x4.Identity);
                Renderer.Render();
            }

            foreach (var child in _children)
            {
                child.Render();
            }
        }

        public int GetRenderLayer() => UIRenderLayer + (int)RenderLayer.Interface;
        public bool ShouldIgnorePostProcessing() => IgnorePostProcessing;

        public void Focus()
        {
            UICanvas.Instance?.Focus(this);
            Focused = true;
        }

        public void Unfocus()
        {
            UICanvas.Instance?.Unfocus(this);
            Focused = false;
        }

        public void Dispose()
        {
            RenderLayerManager.RemoveRenderable(this);

            foreach (var child in _children.ToArray())
            {
                child.Dispose();
            }

            _constraints.Clear();
            OnDispose();
            Renderer?.Dispose();
            UICanvas.Instance?.UnregisterUIElement(this);

            GC.SuppressFinalize(this);
        }

        protected virtual void OnDispose() { }

        ~UIElement()
        {
            Dispose();
        }
    }
}
