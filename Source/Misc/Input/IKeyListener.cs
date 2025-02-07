namespace Electron2D.Misc.Input
{
    public interface IKeyListener
    {
        public void KeyPressed(char code);
        public void KeyNonAlphaReleased(char code);
    }
}
