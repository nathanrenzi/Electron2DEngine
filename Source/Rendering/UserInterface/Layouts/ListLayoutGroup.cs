using System.Numerics;

namespace Electron2D.UserInterface
{
    public class ListLayoutGroup : LayoutGroup
    {
        public Vector4 Padding { get; set; }
        public float Spacing { get; set; }
        public ListDirection Direction { get; set; }
        public ChildSizeMode ChildSizeMode { get; set; }
        public SizeMode SizeMode { get; set; }
        public Vector2 ControlSize { get; set; }
        public LayoutAlignment HorizontalAlignment { get; set; }
        public LayoutAlignment VerticalAlignment { get; set; }
        public SizeMode FitParentToList { get; set; }
        public bool SpaceBetween { get; set; }
        private float _totalChildSize = 0;
        private float _totalChildCrossSize = 0;
        private float _spaceBetween = 0;

        public ListLayoutGroup(Vector4 padding, float spacing, ListDirection direction,
            LayoutAlignment horizontalAlignment = LayoutAlignment.Left,
            LayoutAlignment verticalAlignment = LayoutAlignment.Top,
            ChildSizeMode childSizeMode = ChildSizeMode.None, SizeMode sizeMode = SizeMode.None,
            SizeMode fitParentToList = SizeMode.None, Vector2? controlSize = null, bool spaceBetween = false) : base()
        {
            Padding = padding;
            Spacing = spacing;
            Direction = direction;
            ChildSizeMode = childSizeMode;
            SizeMode = sizeMode;
            ControlSize = controlSize != null ? controlSize.Value : new Vector2(50, 50);
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            FitParentToList = fitParentToList;
            SpaceBetween = spaceBetween;
        }

        protected override void RecalculateLayout()
        {
            SetComponentSizes();
            CalculateChildSizes();
            if(ChildSizeMode == ChildSizeMode.Expand)
            {
                switch (SizeMode)
                {
                    case SizeMode.Width:
                        if (FitParentToList != SizeMode.Width)
                            ApplyFitParentToList();
                        break;
                    case SizeMode.Height:
                        if (FitParentToList != SizeMode.Height)
                            ApplyFitParentToList();
                        break;
                    case SizeMode.None:
                        ApplyFitParentToList();
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (FitParentToList != SizeMode.None)
                    ApplyFitParentToList();
            }
            SetComponentPositions();
            UpdateComponentMeshes();
        }

        private void ApplyFitParentToList()
        {
            float sizeX = _parent.SizeX;
            if(FitParentToList is SizeMode.Width or SizeMode.WidthHeight)
            {
                sizeX = (Direction == ListDirection.Horizontal ? _totalChildSize : _totalChildCrossSize) + Padding.X + Padding.Y;
            }

            float sizeY = _parent.SizeY;
            if(FitParentToList is SizeMode.Height or SizeMode.WidthHeight)
            {
                sizeY = (Direction == ListDirection.Vertical ? _totalChildSize : _totalChildCrossSize) + Padding.Z + Padding.W;
            }

            _parent.SetSize(new Vector2(sizeX, sizeY));
        }

        private void SetComponentSizes()
        {
            if(ChildSizeMode == ChildSizeMode.Expand)
            {
                float expandXSize = _parent.SizeX - (Padding.X + Padding.Y);
                float expandYSize = _parent.SizeY - (Padding.Z + Padding.W);

                if (Direction == ListDirection.Vertical)
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
                    if (SizeMode is SizeMode.Width or SizeMode.WidthHeight)
                    {
                        component.SizeX = expandXSize;
                    }
                    if (SizeMode is SizeMode.Height or SizeMode.WidthHeight)
                    {
                        component.SizeY = expandYSize;
                    }
                }
            }
            else if(ChildSizeMode == ChildSizeMode.Control)
            {
                foreach (var component in Components)
                {
                    if (SizeMode is SizeMode.Width or SizeMode.WidthHeight)
                    {
                        component.SizeX = ControlSize.X;
                    }
                    if (SizeMode is SizeMode.Height or SizeMode.WidthHeight)
                    {
                        component.SizeY = ControlSize.Y;
                    }
                }
            }
        }

        private void CalculateChildSizes()
        {
            _totalChildSize = ((Components.Count - 1) * Spacing);
            _totalChildCrossSize = 0;
            _spaceBetween = Spacing;
            float size = 0;
            foreach (var component in Components)
            {
                size += Direction == ListDirection.Vertical ? component.SizeY : component.SizeX;
                _totalChildCrossSize = MathF.Max(_totalChildCrossSize, Direction == ListDirection.Vertical ? component.SizeX : component.SizeY);
            }
            _totalChildSize += size;
            if(SpaceBetween && Components.Count > 1)
                _spaceBetween = ((Direction == ListDirection.Horizontal ? _parent.SizeX - Padding.X - Padding.Y :
                    _parent.SizeY - Padding.Z - Padding.W) - size) / (Components.Count - 1);
        }

        private void SetComponentPositions()
        {
            float yPosition = 0;
            float xPosition = 0;

            switch (VerticalAlignment)
            {
                case LayoutAlignment.Top:
                    yPosition = _parent.TopYBound + Padding.Z;
                    break;
                case LayoutAlignment.Center:
                    yPosition = Direction == ListDirection.Horizontal ? _parent.TopYBound + (_parent.SizeY - _totalChildCrossSize) / 2f
                        : _parent.TopYBound + (_parent.SizeY - _totalChildSize) / 2f;
                    break;
                case LayoutAlignment.Bottom:
                    yPosition = _parent.BottomYBound - Padding.W - 
                        (Direction == ListDirection.Horizontal ? _totalChildCrossSize : _totalChildSize);
                    break;
            }

            switch (HorizontalAlignment)
            {
                case LayoutAlignment.Left:
                    xPosition = _parent.LeftXBound + Padding.X;
                    break;
                case LayoutAlignment.Center:
                    xPosition = Direction == ListDirection.Horizontal ? _parent.LeftXBound +
                        (_parent.SizeX - _totalChildSize) / 2f : _parent.LeftXBound + (_parent.SizeX - _totalChildCrossSize) / 2f;
                    break;
                case LayoutAlignment.Right:
                    xPosition = _parent.RightXBound - Padding.Y -
                        (Direction == ListDirection.Horizontal ? _totalChildSize : _totalChildCrossSize);
                    break;
            }

            foreach (var component in Components)
            {
                component.Anchor = new Vector2(-1, -1);
                component.Transform.Position = new Vector2(xPosition, yPosition) + _parent.Transform.Position;

                if (Direction == ListDirection.Vertical)
                {
                    yPosition += component.SizeY + _spaceBetween;
                }
                else
                {
                    xPosition += component.SizeX + _spaceBetween;
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