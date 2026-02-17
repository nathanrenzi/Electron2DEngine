using System.Numerics;

namespace Electron2D.UI
{
    public abstract class LinearLayout : ILayout
    {
        public float Spacing { get; set; }
        public LayoutAlignment MainAxisAlignment { get; set; }
        public LayoutAlignment CrossAxisAlignment { get; set; }
        public bool SpaceBetween { get; set; }
        public bool ReverseOrder { get; set; }

        protected abstract float GetMain(Vector2 v);
        protected abstract float GetCross(Vector2 v);
        protected abstract Vector2 MakeVector(float main, float cross);

        public Vector2 Measure(UIElement parent, Vector2 availableSize)
        {
            Vector2 innerSize = new Vector2(
                Math.Max(0, availableSize.X - parent.Padding.Left - parent.Padding.Right),
                Math.Max(0, availableSize.Y - parent.Padding.Top - parent.Padding.Bottom)
            );

            float totalMain = 0;
            float maxCross = 0;
            int visibleCount = 0;

            foreach (var child in parent.Children)
            {
                if (!child.Visible) continue;

                Vector2 childAvailable = MakeVector(float.MaxValue, GetCross(innerSize));

                if (CrossAxisAlignment == LayoutAlignment.Stretch)
                    childAvailable = MakeVector(float.MaxValue, GetCross(innerSize));

                var childDesired = child.Measure(childAvailable);
                totalMain += GetMain(childDesired);
                maxCross = Math.Max(maxCross, GetCross(childDesired));
                visibleCount++;
            }

            if (visibleCount > 1)
                totalMain += Spacing * (visibleCount - 1);

            Vector2 desiredSize = MakeVector(totalMain, maxCross);

            return new Vector2(
                desiredSize.X + parent.Padding.Left + parent.Padding.Right,
                desiredSize.Y + parent.Padding.Top + parent.Padding.Bottom
            );
        }

        public void Arrange(UIElement parent, Rect finalRect)
        {
            Rect innerRect = new Rect(
                finalRect.X + parent.Padding.Left,
                finalRect.Y + parent.Padding.Top,
                Math.Max(0, finalRect.Width - parent.Padding.Left - parent.Padding.Right),
                Math.Max(0, finalRect.Height - parent.Padding.Top - parent.Padding.Bottom)
            );

            float innerMain = GetMain(new Vector2(innerRect.Width, innerRect.Height));
            float innerCross = GetCross(new Vector2(innerRect.Width, innerRect.Height));

            // Collect visible children in order
            List<UIElement> children = new List<UIElement>();
            foreach (var child in parent.Children)
            {
                if (!child.Visible) continue;
                children.Add(child);
            }

            if (ReverseOrder) children.Reverse();

            if (children.Count == 0) return;

            float totalDesiredMain = 0;
            foreach (var child in children)
                totalDesiredMain += GetMain(child.DesiredSize);

            float spacing = Spacing;
            if (SpaceBetween && children.Count > 1)
            {
                float leftover = innerMain - totalDesiredMain;
                spacing = Math.Max(0, leftover / (children.Count - 1));
            }

            float totalSpacing = children.Count > 1 ? spacing * (children.Count - 1) : 0;
            float totalMain = totalDesiredMain + totalSpacing;

            float mainStart = GetMain(new Vector2(innerRect.X, innerRect.Y));
            switch (MainAxisAlignment)
            {
                case LayoutAlignment.Center:
                    mainStart += (innerMain - totalMain) / 2f;
                    break;
                case LayoutAlignment.End:
                    mainStart += innerMain - totalMain;
                    break;
            }

            float crossStart = GetCross(new Vector2(innerRect.X, innerRect.Y));

            // Arrange each child
            float currentMain = mainStart;
            foreach (var child in children)
            {
                float childMain = GetMain(child.DesiredSize);
                float childCross;
                float childCrossOffset;

                if (MainAxisAlignment == LayoutAlignment.Stretch)
                {
                    float stretchMain = (innerMain - totalSpacing) / children.Count;
                    childMain = Math.Max(0, stretchMain - GetMain(new Vector2(child.Margin.Left + child.Margin.Right,
                        child.Margin.Top + child.Margin.Bottom)));
                }

                switch (CrossAxisAlignment)
                {
                    case LayoutAlignment.Stretch:
                        childCross = innerCross - GetCross(new Vector2(
                            child.Margin.Left + child.Margin.Right,
                            child.Margin.Top + child.Margin.Bottom));
                        childCrossOffset = crossStart;
                        break;
                    case LayoutAlignment.Center:
                        childCross = GetCross(child.DesiredSize);
                        childCrossOffset = crossStart + (innerCross - childCross) / 2f;
                        break;
                    case LayoutAlignment.End:
                        childCross = GetCross(child.DesiredSize);
                        childCrossOffset = crossStart + innerCross - childCross;
                        break;
                    default: // Start
                        childCross = GetCross(child.DesiredSize);
                        childCrossOffset = crossStart;
                        break;
                }

                Vector2 childPos = MakeVector(currentMain, childCrossOffset);
                Vector2 childSize = MakeVector(childMain, childCross);

                child.Arrange(new Rect(childPos.X, childPos.Y, childSize.X, childSize.Y));

                currentMain += childMain + spacing;
            }
        }
    }
}
