using System.Numerics;

namespace Electron2D.Core.UserInterface
{
    public class VerticalLayout : Layout
    {
        public Vector4 Padding; // Left, Right, Top, Bottom
        public float Spacing;

        // Control size overrides expand size
        public SizeMode ExpandSizeMode;
        public SizeMode ControlSizeMode;
        public Vector2 ChildSize = new Vector2(50, 50);
        public LayoutAlignment HorizontalAlignment;
        public LayoutAlignment VerticalAlignment;

        public VerticalLayout(Vector4 _padding, float _spacing, SizeMode _expandSizeMode = SizeMode.WidthHeight,
            SizeMode _controlSizeMode = SizeMode.None, LayoutAlignment _horizontalAlignment = LayoutAlignment.Left,
            LayoutAlignment _verticalAlignment = LayoutAlignment.Top)
        {
            Padding = _padding;
            Spacing = _spacing;
            ExpandSizeMode = _expandSizeMode;
            ControlSizeMode = _controlSizeMode;
            HorizontalAlignment = _horizontalAlignment;
            VerticalAlignment = _verticalAlignment;
        }

        public override void RecalculateLayout()
        {
            SetComponentSizes();
            SetComponentPositions();
        }

        private void SetComponentSizes()
        {
            float expandXSize = parent.SizeX - (Padding.X + Padding.Y);
            float expandYSize = parent.SizeY - (Padding.Z + Padding.W + ((components.Count - 1) * Spacing));
            expandYSize /= components.Count;

            foreach (var component in components)
            {
                // Size X
                if(ControlSizeMode is SizeMode.Width or SizeMode.WidthHeight)
                {
                    component.SizeX = ChildSize.X;
                }
                else if(ExpandSizeMode is SizeMode.Width or SizeMode.WidthHeight)
                {
                    if (component.SizeX < expandXSize)
                    {
                        component.SizeX = expandXSize;
                    }
                }

                // Size Y
                if (ControlSizeMode is SizeMode.Height or SizeMode.WidthHeight)
                {
                    component.SizeY = ChildSize.Y;
                }
                else if (ExpandSizeMode is SizeMode.Height or SizeMode.WidthHeight)
                {
                    if (component.SizeY < expandYSize)
                    {
                        component.SizeY = expandYSize;
                    }
                }
            }
        }

        private void SetComponentPositions()
        {
            float yPosition = 0;
            float xPosition = 0;

            Vector2 anchor = Vector2.Zero;

            float totalYSize = ((components.Count - 1) * Spacing);
            foreach (var component in components)
            {
                totalYSize += component.SizeY;
            }

            switch (VerticalAlignment)
            {
                case LayoutAlignment.Top:
                    yPosition = parent.TopYBound - Padding.Z;
                    anchor.Y = 1;
                    break;
                case LayoutAlignment.Center:
                    yPosition = parent.TopYBound - (totalYSize / 2f);
                    anchor.Y = 0;
                    break;
                case LayoutAlignment.Bottom:
                    yPosition = parent.BottomYBound + Padding.W;
                    anchor.Y = -1;
                    break;
            }

            switch (HorizontalAlignment)
            {
                case LayoutAlignment.Left:
                    xPosition = parent.LeftXBound;
                    anchor.X = -1;
                    break;
                case LayoutAlignment.Center:
                    xPosition = 0;
                    anchor.X = 0;
                    break;
                case LayoutAlignment.Right:
                    xPosition = parent.RightXBound;
                    anchor.X = 1;
                    break;
            }

            foreach (var component in components)
            {
                component.Anchor = anchor;
                component.Transform.Position = new Vector2(xPosition, yPosition) + parent.Transform.Position;

                if(VerticalAlignment is LayoutAlignment.Top or LayoutAlignment.Center)
                {
                    yPosition -= component.SizeY + Spacing;
                }
                else if(VerticalAlignment is LayoutAlignment.Bottom)
                {
                    yPosition += component.SizeY + Spacing;
                }
            }
        }
    }
}