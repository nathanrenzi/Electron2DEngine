using System.Numerics;

namespace Electron2D.UserInterface
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
            float expandXSize = _parent.SizeX - (Padding.X + Padding.Y);
            float expandYSize = _parent.SizeY - (Padding.Z + Padding.W);

            if(Direction == ListDirection.Vertical)
            {
                expandYSize -= (Components.Count - 1) * Spacing;
                expandYSize /= Components.Count;
            }
            else
            {
                expandXSize -= (Components.Count - 1) * Spacing;
                expandXSize /= Components.Count;
            }

            foreach (var component in Components)
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

            float totalSize = ((Components.Count - 1) * Spacing);
            foreach (var component in Components)
            {
                totalSize += Direction == ListDirection.Vertical ? component.SizeY : component.SizeX;
            }

            switch (VerticalAlignment)
            {
                case LayoutAlignment.Top:
                    yPosition = _parent.TopYBound + Padding.Z;
                    anchor.Y = -1;
                    break;
                case LayoutAlignment.Center:
                    yPosition = _parent.TopYBound + (_parent.SizeY - totalSize) / 2f;
                    anchor.Y = 0;
                    break;
                case LayoutAlignment.Bottom:
                    yPosition = _parent.BottomYBound - Padding.W - totalSize;
                    anchor.Y = 1;
                    break;
            }

            switch (HorizontalAlignment)
            {
                case LayoutAlignment.Left:
                    xPosition = _parent.LeftXBound + Padding.X;
                    anchor.X = -1;
                    break;
                case LayoutAlignment.Center:
                    xPosition = Direction == ListDirection.Horizontal ? _parent.RightXBound - (_parent.SizeX - totalSize) / 2f : _parent.LeftXBound + _parent.SizeX / 2f;
                    anchor.X = 0;
                    break;
                case LayoutAlignment.Right:
                    xPosition = _parent.RightXBound - Padding.Y;
                    anchor.X = 1;
                    break;
            }

            foreach (var component in Components)
            {
                component.Anchor = anchor;
                component.Transform.Position = new Vector2(xPosition, yPosition) + _parent.Transform.Position;

                if (Direction == ListDirection.Vertical)
                {
                    if (VerticalAlignment == LayoutAlignment.Top || VerticalAlignment == LayoutAlignment.Center)
                    {
                        yPosition += component.SizeY + Spacing;
                    }
                    else if (VerticalAlignment is LayoutAlignment.Bottom)
                    {
                        yPosition -= component.SizeY + Spacing;
                    }
                }
                else
                {
                    if (HorizontalAlignment == LayoutAlignment.Right || HorizontalAlignment == LayoutAlignment.Center)
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
            foreach (var component in Components)
            {
                component.UpdateMesh();
            }
        }
    }
}