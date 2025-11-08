namespace Electron2D.UserInterface
{
    public abstract class UIConstraint
    {
        public abstract void ApplyConstraint(UIComponent component);
        public abstract bool CheckConstraint(UIComponent component);
    }
}
