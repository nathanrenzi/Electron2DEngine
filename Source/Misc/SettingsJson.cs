using Electron2D.Rendering;

namespace Electron2D.Misc
{
    public class SettingsJson
    {
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public WindowMode WindowMode { get; set; }
        public bool Vsync { get; set; }
        public AntialiasingMode AntialiasingMode { get; set; }
        public int RefreshRate { get; set; }
        public float AudioMasterVolume { get; set; }

        public SettingsJson(int windowWidth, int windowHeight,
            WindowMode windowMode, bool vsync, AntialiasingMode antialiasingMode,
            int refreshRate, float audioMasterVolume)
        {
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            WindowMode = windowMode;
            Vsync = vsync;
            AntialiasingMode = antialiasingMode;
            RefreshRate = refreshRate;
            AudioMasterVolume = audioMasterVolume;
        }
    }
}