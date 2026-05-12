using System.Numerics;

namespace Electron2D.UI
{
    /// <summary>
    /// Represents a UI event dispatched through the element tree.
    /// </summary>
    public sealed class UIEvent
    {
        /// <summary>
        /// The type of this event.
        /// </summary>
        public UIEventType Type { get; set; }

        /// <summary>
        /// The element that originally triggered this event.
        /// </summary>
        public UIElement Target { get; set; }

        /// <summary>
        /// The element currently processing this event during propagation.
        /// </summary>
        public UIElement Current { get; set; }

        /// <summary>
        /// The current propagation phase of this event.
        /// </summary>
        public EventPhase Phase { get; set; } = EventPhase.Bubble;

        /// <summary>
        /// Whether propagation of this event has been stopped.
        /// </summary>
        public bool IsPropagationStopped { get; private set; }

        /// <summary>
        /// The mouse position in virtual space when this event was raised.
        /// </summary>
        public Vector2 MousePosition { get; set; }

        /// <summary>
        /// The change in mouse position since the last event.
        /// </summary>
        public Vector2 MouseDelta { get; set; }

        /// <summary>
        /// The mouse scroll wheel delta when this event was raised.
        /// </summary>
        public float MouseScrollDelta { get; set; }

        /// <summary>
        /// The mouse button associated with this event.
        /// </summary>
        public MouseButton MouseButton { get; set; }

        /// <summary>
        /// Stops this event from propagating further through the element tree.
        /// </summary>
        public void StopPropagation()
        {
            IsPropagationStopped = true;
        }
    }
}
