using Electron2D.Misc;
using Electron2D.Rendering;
using GLFW;
using Newtonsoft.Json;

namespace Electron2D
{
    public class Settings
    {
        public int WindowWidth { get; }
        public int WindowHeight { get; }
        public WindowMode WindowMode { get; }
        public bool Vsync { get; }
        public AntialiasingMode AntialiasingMode { get; }
        public int RefreshRate { get; }
        public float AudioMasterVolume { get; }

        private Settings(SettingsJson json)
        {
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
                if(!File.Exists("DefaultSettings.json"))
                {
                    Debug.LogError("Missing default settings file!");
                    Engine.Game.Exit();
                }

                File.Copy("DefaultSettings.json", "Settings.json");
                json = File.ReadAllText("Settings.json");
                setDisplaySettings = true;
            }

            SettingsJson settingsJson = JsonConvert.DeserializeObject<SettingsJson>(json);
            if (settingsJson == null)
            {
                Debug.LogError("Error reading settings file.");
                Engine.Game.Exit();
            }
            if (setDisplaySettings)
            {
                settingsJson.WindowWidth = Glfw.PrimaryMonitor.WorkArea.Width;
                settingsJson.WindowHeight = Glfw.PrimaryMonitor.WorkArea.Height;
                settingsJson.RefreshRate = Glfw.GetVideoMode(Glfw.PrimaryMonitor).RefreshRate;
            }
            Settings s = new Settings(settingsJson);
            s.WriteToJson();
            return s;
        }

        private void WriteToJson()
        {
            SettingsJson settingsJson = new SettingsJson(WindowWidth, WindowHeight, WindowMode,
                Vsync, AntialiasingMode, RefreshRate, AudioMasterVolume);
            string json = JsonConvert.SerializeObject(settingsJson, Formatting.Indented);
            File.WriteAllText("Settings.json", json);
        }
    }
}
