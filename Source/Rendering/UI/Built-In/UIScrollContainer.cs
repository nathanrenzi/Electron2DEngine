using System.Numerics;

namespace Atlas2D.UI
{
    /// <summary>
    /// A UI element that allows its content to be scrolled when it exceeds the available space.
    /// </summary>
    public sealed class UIScrollContainer : UIElement
    {
        /// <summary>
        /// The scroll speed in pixels per scroll tick.
        /// </summary>
        public float ScrollSpeed { get; set; } = 200f;

        /// <summary>
        /// Whether the content can be scrolled horizontally.
        /// </summary>
        public bool CanScrollX { get; set; } = true;

        /// <summary>
        /// Whether the content can be scrolled vertically.
        /// </summary>
        public bool CanScrollY { get; set; } = true;

        /// <summary>
        /// Controls how the content width is sized relative to the container.
        /// </summary>
        public ScrollContentSizing ContentSizingX { get; set; } = ScrollContentSizing.None;

        /// <summary>
        /// Controls how the content height is sized relative to the container.
        /// </summary>
        public ScrollContentSizing ContentSizingY { get; set; } = ScrollContentSizing.None;

        /// <summary>
        /// The current scroll offset in pixels.
        /// </summary>
        public Vector2 ScrollOffset { get; private set; }

        /// <summary>
        /// The maximum scroll offset based on the current content and container size.
        /// </summary>
        public Vector2 MaxScrollOffset { get; private set; }

        /// <summary>
        /// The scrollable content container.
        /// </summary>
        public UIContainer Content { get; }

        /// <summary>
        /// The background track of the vertical scrollbar, or <see langword="null"/> if not created.
        /// </summary>
        public UIElement? VerticalBackground => _verticalBackground;

        /// <summary>
        /// The draggable handle of the vertical scrollbar, or <see langword="null"/> if not created.
        /// </summary>
        public UIElement? VerticalHandle => _verticalHandle;

        /// <summary>
        /// The background track of the horizontal scrollbar, or <see langword="null"/> if not created.
        /// </summary>
        public UIElement? HorizontalBackground => _horizontalBackground;

        /// <summary>
        /// The draggable handle of the horizontal scrollbar, or <see langword="null"/> if not created.
        /// </summary>
        public UIElement? HorizontalHandle => _horizontalHandle;

        private readonly UIScrollContainerStyle? _style;

        private readonly UIElement? _verticalBackground;
        private readonly UIElement? _verticalHandle;
        private readonly UIElement? _horizontalBackground;
        private readonly UIElement? _horizontalHandle;

        private float _verticalTrackLength;
        private float _horizontalTrackLength;
        private float _verticalHandleSize;
        private float _horizontalHandleSize;

        private Vector2 _dragStartMouse;
        private Vector2 _dragStartScroll;

        /// <summary>
        /// Creates a new <see cref="UIScrollContainer"/>.
        /// </summary>
        /// <param name="style">The visual style of the scrollbars, or <see langword="null"/> for no scrollbars.</param>
        public UIScrollContainer(UIScrollContainerStyle? style = null, UIRenderArgs? arguments = null)
            : base(arguments.HasValue ? new UIRenderArgs(arguments.Value) { Mask = true } : new UIRenderArgs() { Mask = true }, false)
        {
            Content = new UIContainer(arguments)
            {
                IgnoreLayout = true,
                Anchor = Vector2.Zero,
                Pivot = Vector2.Zero
            };

            AddChild(Content);

            _style = style;

            if(_style != null)
            {
                if(_style.VerticalVisibility != ScrollBarVisibility.Never)
                {
                    if(_style.BackgroundDef != null)
                    {
                        _verticalBackground = _style.BackgroundDef.Create(arguments);
                        _verticalBackground.Interactable = false;
                        _verticalBackground.IgnoreLayout = true;
                        AddChild(_verticalBackground);
                    }

                    _verticalHandle = _style.HandleDef.Create(arguments);
                    _verticalHandle.SetHoverCursorType(GLFW.CursorType.Hand);
                    _verticalHandle.IgnoreLayout = true;
                    _verticalHandle.AddEventListener(UIEventType.DragStart, OnVerticalHandleDragStart);
                    _verticalHandle.AddEventListener(UIEventType.Drag, OnVerticalHandleDrag);
                    _verticalHandle.AddEventListener(UIEventType.DragEnd, OnHandleDragEnd);
                    AddChild(_verticalHandle);
                }

                if(_style.HorizontalVisibility != ScrollBarVisibility.Never)
                {
                    if(_style.BackgroundDef != null)
                    {
                        _horizontalBackground = _style.BackgroundDef.Create(arguments);
                        _horizontalBackground.Interactable = false;
                        _horizontalBackground.IgnoreLayout = true;
                        AddChild(_horizontalBackground);
                    }

                    _horizontalHandle = _style.HandleDef.Create(arguments);
                    _horizontalHandle.SetHoverCursorType(GLFW.CursorType.Hand);
                    _horizontalHandle.IgnoreLayout = true;
                    _horizontalHandle.AddEventListener(UIEventType.DragStart, OnHorizontalHandleDragStart);
                    _horizontalHandle.AddEventListener(UIEventType.Drag, OnHorizontalHandleDrag);
                    _horizontalHandle.AddEventListener(UIEventType.DragEnd, OnHandleDragEnd);
                    AddChild(_horizontalHandle);
                }
            }

            AddEventListener(UIEventType.MouseScroll, OnMouseScroll);
        }

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            availableSize = new Vector2(
                Math.Max(0, availableSize.X - Padding.Left - Padding.Right),
                Math.Max(0, availableSize.Y - Padding.Top - Padding.Bottom)
            );

            Vector2 contentConstraint = new Vector2(
                CanScrollX ? float.PositiveInfinity : availableSize.X,
                CanScrollY ? float.PositiveInfinity : availableSize.Y
            );

            Content.Measure(contentConstraint);
            Vector2 contentSize = Content.DesiredSize;

            float x = ContentSizingX switch
            {
                ScrollContentSizing.Match => availableSize.X,
                ScrollContentSizing.Min => Math.Max(contentSize.X, availableSize.X),
                _ => contentSize.X
            };

            float y = ContentSizingY switch
            {
                ScrollContentSizing.Match => availableSize.Y,
                ScrollContentSizing.Min => Math.Max(contentSize.Y, availableSize.Y),
                _ => contentSize.Y
            };

            return new Vector2(
                x + Padding.Left + Padding.Right,
                y + Padding.Top + Padding.Bottom
            );
        }

        protected override void ArrangeCore(Rect finalRect)
        {
            Rect childRect = new Rect(
                Padding.Left,
                Padding.Top,
                Math.Max(0, finalRect.Width - Padding.Left - Padding.Right),
                Math.Max(0, finalRect.Height - Padding.Top - Padding.Bottom)
            );

            Vector2 contentSize = Content.DesiredSize;

            float x = ContentSizingX switch
            {
                ScrollContentSizing.Match => Size.X,
                ScrollContentSizing.Min => Math.Max(contentSize.X, Size.X),
                _ => contentSize.X
            };

            float y = ContentSizingY switch
            {
                ScrollContentSizing.Match => Size.Y,
                ScrollContentSizing.Min => Math.Max(contentSize.Y, Size.Y),
                _ => contentSize.Y
            };

            bool snapToEndX = ScrollOffset.X == MaxScrollOffset.X;
            bool snapToEndY = ScrollOffset.Y == MaxScrollOffset.Y;

            MaxScrollOffset = new Vector2(
                CanScrollX ? Math.Max(0, x - Size.X) : 0,
                CanScrollY ? Math.Max(0, y - Size.Y) : 0
            );

            float scrollOffsetX = snapToEndX ? MaxScrollOffset.X
                : Math.Clamp(ScrollOffset.X, 0, MaxScrollOffset.X);
            float scrollOffsetY = snapToEndY ? MaxScrollOffset.Y
                : Math.Clamp(ScrollOffset.Y, 0, MaxScrollOffset.Y);

            ScrollOffset = new Vector2(scrollOffsetX, scrollOffsetY);

            Rect contentRect = new Rect(
                childRect.X - ScrollOffset.X,
                childRect.Y - ScrollOffset.Y,
                x,
                y
            );

            Content.Arrange(contentRect);

            Rect scrollBarRect = new Rect(
                childRect.X + Content.Padding.Left,
                childRect.Y + Content.Padding.Top,
                childRect.Width - Content.Padding.Left - Content.Padding.Right,
                childRect.Height - Content.Padding.Top - Content.Padding.Bottom
            );

            if (_style != null)
                ArrangeScrollBars(scrollBarRect);
        }

        private void ArrangeScrollBars(Rect innerRect)
        {
            float thickness = _style!.Thickness;

            bool bothVisible = _verticalHandle != null && _horizontalHandle != null
                && (_style.VerticalVisibility == ScrollBarVisibility.Always || MaxScrollOffset.Y > 0)
                && (_style.HorizontalVisibility == ScrollBarVisibility.Always || MaxScrollOffset.X > 0);

            float vTrackH = innerRect.Height - (bothVisible ? thickness : 0f);
            float hTrackW = innerRect.Width - (bothVisible ? thickness : 0f);

            if (_verticalHandle != null)
            {
                bool visible = _style.VerticalVisibility == ScrollBarVisibility.Always
                    || MaxScrollOffset.Y > 0;

                _verticalHandle.Visible = visible;
                if (_verticalBackground != null) _verticalBackground.Visible = visible;

                float trackX = innerRect.X + innerRect.Width - thickness; ;
                _verticalTrackLength = vTrackH;
                _verticalHandleSize = Math.Max(_style.MinHandleSize, vTrackH * (vTrackH / (vTrackH + MaxScrollOffset.Y)));

                float handleY = MaxScrollOffset.Y > 0
                    ? (_verticalTrackLength - _verticalHandleSize) * (ScrollOffset.Y / MaxScrollOffset.Y)
                    : 0f;

                if (_verticalBackground != null)
                    _verticalBackground.Arrange(new Rect(trackX, innerRect.Y, thickness, vTrackH));

                _verticalHandle.Arrange(new Rect(trackX, innerRect.Y + handleY, thickness, _verticalHandleSize));
            }

            if (_horizontalHandle != null)
            {
                bool visible = _style.HorizontalVisibility == ScrollBarVisibility.Always
                    || MaxScrollOffset.X > 0;

                _horizontalHandle.Visible = visible;
                if (_horizontalBackground != null) _horizontalBackground.Visible = visible;

                float trackY = innerRect.Y + innerRect.Height - thickness;
                _horizontalTrackLength = hTrackW;
                _horizontalHandleSize = Math.Max(_style.MinHandleSize, hTrackW * (hTrackW / (hTrackW + MaxScrollOffset.X)));

                float handleX = MaxScrollOffset.X > 0
                    ? (_horizontalTrackLength - _horizontalHandleSize) * (ScrollOffset.X / MaxScrollOffset.X)
                    : 0f;

                if (_horizontalBackground != null)
                    _horizontalBackground.Arrange(new Rect(innerRect.X, trackY, hTrackW, thickness));

                _horizontalHandle.Arrange(new Rect(innerRect.X + handleX, trackY, _horizontalHandleSize, thickness));
            }
        }

        private void OnVerticalHandleDragStart(UIEvent evt)
        {
            _dragStartMouse = evt.MousePosition;
            _dragStartScroll = ScrollOffset;
        }

        private void OnVerticalHandleDrag(UIEvent evt)
        {
            float trackLength = _verticalTrackLength - _verticalHandleSize;
            if (trackLength <= 0) return;
            float delta = (evt.MousePosition.Y - _dragStartMouse.Y) / trackLength;
            SetScrollOffset(new Vector2(ScrollOffset.X, _dragStartScroll.Y + delta * MaxScrollOffset.Y));
        }

        private void OnHorizontalHandleDragStart(UIEvent evt)
        {
            _dragStartMouse = evt.MousePosition;
            _dragStartScroll = ScrollOffset;
        }

        private void OnHorizontalHandleDrag(UIEvent evt)
        {
            float trackLength = _horizontalTrackLength - _horizontalHandleSize;
            if (trackLength <= 0) return;
            float delta = (evt.MousePosition.X - _dragStartMouse.X) / trackLength;
            SetScrollOffset(new Vector2(_dragStartScroll.X + delta * MaxScrollOffset.X, ScrollOffset.Y));
        }

        private void OnHandleDragEnd(UIEvent evt)
        {
            _dragStartMouse = Vector2.Zero;
            _dragStartScroll = Vector2.Zero;
        }

        private void OnMouseScroll(UIEvent evt)
        {
            Vector2 delta = new Vector2(
                CanScrollX ? -evt.MouseScrollDelta * ScrollSpeed : 0,
                CanScrollY ? -evt.MouseScrollDelta * ScrollSpeed : 0
            );

            SetScrollOffset(ScrollOffset + delta);
        }

        /// <summary>
        /// Sets the scroll offset, clamped to the valid scroll range.
        /// </summary>
        /// <param name="offset">The desired scroll offset in pixels.</param>
        public void SetScrollOffset(Vector2 offset)
        {
            ScrollOffset = new Vector2(
                Math.Clamp(offset.X, 0, MaxScrollOffset.X),
                Math.Clamp(offset.Y, 0, MaxScrollOffset.Y)
            );
            InvalidateMeasure();
        }

        public override void UpdateMesh() { }
    }
}