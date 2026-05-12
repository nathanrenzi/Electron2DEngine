namespace Electron2D.Misc.Input
{
    /// <summary>
    /// Defines a listener that receives keyboard input events.
    /// </summary>
    public interface IKeyListener
    {
        /// <summary>
        /// Called when a keyboard event occurs.
        /// </summary>
        /// <param name="keyEvent">The key event to process.</param>
        public void OnKeyEvent(KeyEvent keyEvent);
    }
}
