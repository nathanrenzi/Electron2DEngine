using Electron2D.Core.UI;

namespace Electron2D.Core.UserInterface
{
    public abstract class UiConstraint
    {
        public UiConstraintSide side { get; protected set; }
        protected float LeftWindowBound { get { return -Game.REFERENCE_WINDOW_WIDTH / 2f; } }
        protected float RightWindowBound { get { return Game.REFERENCE_WINDOW_WIDTH / 2f; } }
        protected float TopWindowBound { get { return Game.REFERENCE_WINDOW_HEIGHT / 2f; } }
        protected float BottomWindowBound { get { return -Game.REFERENCE_WINDOW_HEIGHT / 2f; } }

        public abstract void ApplyConstraint(UiComponent _component);
    }
}
