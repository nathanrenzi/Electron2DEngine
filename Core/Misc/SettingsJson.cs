using Electron2D.Core.Rendering;

namespace Electron2D.Core.Misc
{
    public class SettingsJson
    {
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public string WindowTitle { get; set; }
        public WindowMode WindowMode { get; set; }
        public bool Vsync { get; set; }
        public AntialiasingMode AntialiasingMode { get; set; }
        public int RefreshRate { get; set; }

        public SettingsJson(int windowWidth, int windowHeight, string windowTitle,
            WindowMode windowMode, bool vsync, AntialiasingMode antialiasingMode,
            int refreshRate)
        {
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            WindowTitle = windowTitle;
            WindowMode = windowMode;
            Vsync = vsync;
            AntialiasingMode = antialiasingMode;
            RefreshRate = refreshRate;
        }
    }
}