namespace Electron2D.Misc.Input
{
    public sealed class KeyEvent
    {
        public KeyEventType Type;
        public KeyCode KeyCode;
        public char? Character;
        public bool IsPressed;
    }
}
