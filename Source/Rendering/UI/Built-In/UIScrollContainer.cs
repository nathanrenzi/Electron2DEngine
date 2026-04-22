using System.Numerics;

namespace Electron2D.UI
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


        public UIScrollContainer(UIRenderArgs? arguments = null)
            : base(arguments.HasValue ? new UIRenderArgs(arguments.Value) { Mask = true } : new UIRenderArgs() { Mask = true }, false)
        {
            Content = new UIContainer(arguments)
            {
                IgnoreLayout = true,
                Anchor = Vector2.Zero,
                Pivot = Vector2.Zero
            };

            AddChild(Content);
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