namespace Atlas2D.UI
{
    /// <summary>
    /// Defines the phase of an event as it propagates through the UI element tree.
    /// </summary>
    public enum EventPhase
    {
        /// <summary>
        /// The event is travelling down the tree from the root toward the target element.
        /// </summary>
        Capture,

        /// <summary>
        /// The event has reached the target element.
        /// </summary>
        Target,

        /// <summary>
        /// The event is travelling back up the tree from the target element toward the root.
        /// </summary>
        Bubble
    }
}
