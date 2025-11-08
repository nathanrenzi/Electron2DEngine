using System.Numerics;

namespace Electron2D.UserInterface
{
    public class ContainLayoutGroup : LayoutGroup
    {
        public Vector4 Padding;

        public ContainLayoutGroup(Vector4 padding)
        {
            Padding = padding;
        }

        protected override void RecalculateLayout()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                UIComponent component = Components[i];

                component.Anchor = new Vector2(-1, -1);
                component.SizeX = _parent.SizeX - (Padding.X + Padding.Y);
                component.SizeY = _parent.SizeY - (Padding.Z + Padding.W);
                component.Transform.Position = new Vector2(_parent.LeftXBound + Padding.X, _parent.TopYBound + Padding.Z) + _parent.Transform.Position;
            }
        }
    }
}