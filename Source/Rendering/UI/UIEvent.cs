using System.Numerics;

namespace Electron2D.UI
{
    public class UIEvent
    {
        public UIEventType Type { get; set; }
        public UIElement Target { get; set; }
        public UIElement Current { get; set; }
        public EventPhase Phase { get; set; } = EventPhase.Bubble;
        public bool IsPropagationStopped { get; private set; }

        public Vector2 MousePosition { get; set; }
        public Vector2 MouseDelta { get; set; }
        public float MouseScrollDelta { get; set; }
        public MouseButton MouseButton { get; set; }

        public void StopPropagation()
        {
            IsPropagationStopped = true;
        }
    }
}
