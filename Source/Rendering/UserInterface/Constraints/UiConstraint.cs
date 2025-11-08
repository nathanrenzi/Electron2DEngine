namespace Electron2D.UserInterface
{
    public abstract class UIConstraint
    {
        public UIConstraintSide Side { get; protected set; }
        protected float LeftWindowBound => -Display.WindowSize.X / 2f;
        protected float RightWindowBound => Display.WindowSize.X / 2f;
        protected float TopWindowBound => Display.WindowSize.Y / 2f;
        protected float BottomWindowBound => -Display.WindowSize.Y / 2f;

        public abstract void ApplyConstraint(UIComponent component);
    }
}
