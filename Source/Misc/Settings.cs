using Electron2D.Rendering;
using GLFW;
using Newtonsoft.Json;

namespace Electron2D
{
    public class Settings
    {
        public class SettingsJson
        {
            public int Monitor { get; set; }
            public int WindowWidth { get; set; }
            public int WindowHeight { get; set; }
            public WindowMode WindowMode { get; set; }
            public bool Vsync { get; set; }
            public AntialiasingMode AntialiasingMode { get; set; }
            public int RefreshRate { get; set; }
            public float AudioMasterVolume { get; set; }

            public SettingsJson()
            {
                Monitor = (int)Glfw.PrimaryMonitor.UserPointer;
                WindowHeight = 0;
                WindowWidth = 0;
                WindowMode = WindowMode.Fullscreen;
                Vsync = true;
                AntialiasingMode = AntialiasingMode.FXAA;
                RefreshRate = 0;
                AudioMasterVolume = 1;
            }

            public SettingsJson(int monitor, int windowWidth, int windowHeight,
                WindowMode windowMode, bool vsync, AntialiasingMode antialiasingMode,
                int refreshRate, float audioMasterVolume)
            {
                Monitor = monitor;
                WindowWidth = windowWidth;
                WindowHeight = windowHeight;
                WindowMode = windowMode;
                Vsync = vsync;
                AntialiasingMode = antialiasingMode;
                RefreshRate = refreshRate;
                AudioMasterVolume = audioMasterVolume;
            }
        }

        public GLFW.Monitor Monitor { get; set; }
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public WindowMode WindowMode { get; set; }
        public bool Vsync { get; set; }
        public AntialiasingMode AntialiasingMode { get; set; }
        public int RefreshRate { get; set; }
        public float AudioMasterVolume { get; set; }

        private Settings(SettingsJson json)
        {
            Monitor = Glfw.PrimaryMonitor;
            for (int i = 0; i < Glfw.Monitors.Length; i++)
            {
                if (Glfw.Monitors[i].UserPointer == json.Monitor)
                {
                    Monitor = Glfw.Monitors[i];
                    break;
                }
            }
            WindowWidth = json.WindowWidth;
            WindowHeight = json.WindowHeight;
            WindowMode = json.WindowMode;
            Vsync = json.Vsync;
            AntialiasingMode = json.AntialiasingMode;
            RefreshRate = json.RefreshRate;
            AudioMasterVolume = json.AudioMasterVolume;
        }

        public static Settings LoadSettingsFile()
        {
            string json;
            bool setDisplaySettings = false;
            if(File.Exists("Settings.json"))
            {
                json = File.ReadAllText("Settings.json");
            }
            else
            {
                json = "";
                setDisplaySettings = true;
            }

            SettingsJson settingsJson = string.IsNullOrEmpty(json) ? new SettingsJson() : JsonConvert.DeserializeObject<SettingsJson>(json);
            if (settingsJson == null)
            {
                Debug.LogError("Error reading settings file.");
                Engine.Game.Exit();
            }

            GLFW.Monitor monitor = GLFW.Monitor.None;
            bool foundMonitor = false;
            for (int i = 0; i < Glfw.Monitors.Length; i++)
            {
                if (Glfw.Monitors[i].UserPointer == settingsJson.Monitor)
                {
                    monitor = Glfw.Monitors[i];
                    foundMonitor = true;
                    break;
                }
            }
            if (!foundMonitor)
            {
                monitor = Glfw.PrimaryMonitor;
                settingsJson.Monitor = (int)Glfw.PrimaryMonitor.UserPointer;
            }

            if (settingsJson.WindowWidth == 0 || settingsJson.WindowHeight == 0 || settingsJson.RefreshRate == 0)
            {
                settingsJson.WindowWidth = monitor.WorkArea.Width;
                settingsJson.WindowHeight = monitor.WorkArea.Height;
                settingsJson.RefreshRate = Glfw.GetVideoMode(monitor).RefreshRate;
            }

            Settings s = new Settings(settingsJson);
            s.WriteToJson();
            return s;
        }

        private void WriteToJson()
        {
            SettingsJson settingsJson = new SettingsJson((int)Monitor.UserPointer, WindowWidth, WindowHeight, WindowMode,
                Vsync, AntialiasingMode, RefreshRate, AudioMasterVolume);
            string json = JsonConvert.SerializeObject(settingsJson, Formatting.Indented);
            File.WriteAllText("Settings.json", json);
        }
    }
}
