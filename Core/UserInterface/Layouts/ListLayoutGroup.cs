using System.Numerics;

namespace Electron2D.Core.UserInterface
{
    public class ListLayoutGroup : LayoutGroup
    {
        public Vector4 Padding; // Left, Right, Top, Bottom
        public float Spacing;

        // Control size overrides expand size
        public ListDirection Direction;
        public SizeMode ExpandSizeMode;
        public SizeMode ControlSizeMode;
        public Vector2 ChildSize = new Vector2(50, 50);
        public LayoutAlignment HorizontalAlignment;
        public LayoutAlignment VerticalAlignment;

        public ListLayoutGroup(Vector4 _padding, float _spacing, ListDirection _direction, SizeMode _expandSizeMode = SizeMode.WidthHeight,
            SizeMode _controlSizeMode = SizeMode.None, LayoutAlignment _horizontalAlignment = LayoutAlignment.Left,
            LayoutAlignment _verticalAlignment = LayoutAlignment.Top) : base()
        {
            Direction = _direction;
            ExpandSizeMode = _expandSizeMode;
            ControlSizeMode = _controlSizeMode;
            Padding = _padding;
            Spacing = _spacing;
            HorizontalAlignment = _horizontalAlignment;
            VerticalAlignment = _verticalAlignment;
        }

        protected override void RecalculateLayout()
        {
            SetComponentSizes();
            SetComponentPositions();
            UpdateComponentMeshes();
        }

        private void SetComponentSizes()
        {
            float expandXSize = parent.SizeX - (Padding.X + Padding.Y);
            float expandYSize = parent.SizeY - (Padding.Z + Padding.W);

            if(Direction == ListDirection.Vertical)
            {
                expandYSize -= (components.Count - 1) * Spacing;
                expandYSize /= components.Count;
            }
            else
            {
                expandXSize -= (components.Count - 1) * Spacing;
                expandXSize /= components.Count;
            }

            foreach (var component in components)
            {
                // Size X
                if(ControlSizeMode is SizeMode.Width or SizeMode.WidthHeight)
                {
                    component.SizeX = ChildSize.X;
                }
                else if(ExpandSizeMode is SizeMode.Width or SizeMode.WidthHeight)
                {
                    component.SizeX = expandXSize;
                }

                // Size Y
                if (ControlSizeMode is SizeMode.Height or SizeMode.WidthHeight)
                {
                    component.SizeY = ChildSize.Y;
                }
                else if (ExpandSizeMode is SizeMode.Height or SizeMode.WidthHeight)
                {
                    component.SizeY = expandYSize;
                }
            }
        }

        private void SetComponentPositions()
        {
            float yPosition = 0;
            float xPosition = 0;

            Vector2 anchor = Vector2.Zero;

            float totalSize = ((components.Count - 1) * Spacing);
            foreach (var component in components)
            {
                totalSize += Direction == ListDirection.Vertical ? component.SizeY : component.SizeX;
            }

            switch (VerticalAlignment)
            {
                case LayoutAlignment.Top:
                    yPosition = parent.TopYBound - Padding.Z;
                    anchor.Y = 1;
                    break;
                case LayoutAlignment.Center:
                    yPosition = Direction == ListDirection.Vertical ? parent.TopYBound - (totalSize / 2f) : 0;
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
                    xPosition = parent.LeftXBound + Padding.X;
                    anchor.X = -1;
                    break;
                case LayoutAlignment.Center:
                    xPosition = Direction == ListDirection.Horizontal ? parent.RightXBound - (totalSize / 2f) : 0;
                    anchor.X = 0;
                    break;
                case LayoutAlignment.Right:
                    xPosition = parent.RightXBound - Padding.Y;
                    anchor.X = 1;
                    break;
            }

            foreach (var component in components)
            {
                component.Anchor = anchor;
                component.Transform.Position = new Vector2(xPosition, yPosition) + parent.Transform.Position;

                if(Direction == ListDirection.Vertical)
                {
                    if (VerticalAlignment is LayoutAlignment.Top or LayoutAlignment.Center)
                    {
                        yPosition -= component.SizeY + Spacing;
                    }
                    else if (VerticalAlignment is LayoutAlignment.Bottom)
                    {
                        yPosition += component.SizeY + Spacing;
                    }
                }
                else // Horizontal
                {
                    if (HorizontalAlignment is LayoutAlignment.Right or LayoutAlignment.Center)
                    {
                        xPosition -= component.SizeX + Spacing;
                    }
                    else if (HorizontalAlignment is LayoutAlignment.Left)
                    {
                        xPosition += component.SizeX + Spacing;
                    }
                }
            }
        }

        private void UpdateComponentMeshes()
        {
            foreach (var component in components)
            {
                component.UpdateMesh();
            }
        }
    }
}