namespace Electron2D.UserInterface
{
    public abstract class UIConstraint
    {
        public UIConstraintSide side { get; protected set; }
        protected float LeftWindowBound { get { return -Display.WindowSize.X / 2f / Display.WindowScale; } }
        protected float RightWindowBound { get { return Display.WindowSize.X / 2f / Display.WindowScale; } }
        protected float TopWindowBound { get { return Display.WindowSize.Y / 2f / Display.WindowScale; } }
        protected float BottomWindowBound { get { return -Display.WindowSize.Y / 2f / Display.WindowScale; } }

        public abstract void ApplyConstraint(UIComponent _component);
    }
}
