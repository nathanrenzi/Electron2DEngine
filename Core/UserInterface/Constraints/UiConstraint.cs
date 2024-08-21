using Electron2D.Core.UserInterface;

namespace Electron2D.Core.UserInterface
{
    public abstract class UiConstraint
    {
        public UiConstraintSide side { get; protected set; }
        protected float LeftWindowBound { get { return -Program.Game.CurrentWindowWidth / 2f / Game.WINDOW_SCALE; } }
        protected float RightWindowBound { get { return Program.Game.CurrentWindowWidth / 2f / Game.WINDOW_SCALE; } }
        protected float TopWindowBound { get { return Program.Game.CurrentWindowHeight / 2f / Game.WINDOW_SCALE; } }
        protected float BottomWindowBound { get { return -Program.Game.CurrentWindowHeight / 2f / Game.WINDOW_SCALE; } }

        public abstract void ApplyConstraint(UiComponent _component);
    }
}
