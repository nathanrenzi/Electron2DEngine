namespace Electron2D.UserInterface
{
    // Remove this class in favor of creating a reference to a canvas in the build

    public static class UI
    {
        public static UICanvas MainCanvas { get; private set; } = new UICanvas();
    }
}
