using System.Numerics;

namespace Electron2D.UserInterface
{
    public class RelativeConstraint : UIConstraint
    {
        public Vector2 Offset { get; set; }
        private readonly UIComponent _relativeComponent;

        public RelativeConstraint(UIComponent relativeComponent, Vector2 offset)
        {
            Offset = offset;
            _relativeComponent = relativeComponent;
        }

        public override void ApplyConstraint(UIComponent component)
        {
            component.Transform.Position = _relativeComponent.Transform.Position + Offset;
        }

        public override bool CheckConstraint(UIComponent component)
        {
            return component.Transform.Position == _relativeComponent.Transform.Position + Offset;
        }
    }
}