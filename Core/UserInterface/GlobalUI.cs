namespace Electron2D.Core.UserInterface
{
    // Remove this class in favor of creating a reference to a canvas in the build

    public static class GlobalUI
    {
        public static UiCanvas MainCanvas { get; private set; } = new UiCanvas();
    }
}
