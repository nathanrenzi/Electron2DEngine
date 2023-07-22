using Electron2D.Core.UI;

namespace Electron2D.Core.UserInterface
{
    public abstract class UiConstraint
    {
        public UiConstraintSide side { get; protected set; }
        protected float LeftWindowBound { get { return -Program.game.currentWindowWidth / 2f; } }
        protected float RightWindowBound { get { return Program.game.currentWindowWidth / 2f; } }
        protected float TopWindowBound { get { return Program.game.currentWindowHeight / 2f; } }
        protected float BottomWindowBound { get { return -Program.game.currentWindowHeight / 2f; } }

        public abstract void ApplyConstraint(UiComponent _component);
    }
}
