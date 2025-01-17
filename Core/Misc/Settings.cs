using Electron2D.Core.Misc;
using Electron2D.Core.Rendering;
using GLFW;
using Newtonsoft.Json;

namespace Electron2D.Core
{
    public class Settings
    {
        // Graphics Settings
        public int WindowWidth { get; }
        public int WindowHeight { get; }
        public string WindowTitle { get; }
        public WindowMode WindowMode { get; }
        public bool Vsync { get; }
        public AntialiasingMode AntialiasingMode { get; }
        public int RefreshRate { get; }

        private Settings(SettingsJson json)
        {
            WindowWidth = json.WindowWidth;
            WindowHeight = json.WindowHeight;
            WindowTitle = json.WindowTitle;
            WindowMode = json.WindowMode;
            Vsync = json.Vsync;
            AntialiasingMode = json.AntialiasingMode;
            RefreshRate = json.RefreshRate;
        }

        public static Settings LoadSettingsFile()
        {
            string json;
            bool setDisplaySettings = false;
            if(File.Exists("Core/Settings.json"))
            {
                json = File.ReadAllText("Core/Settings.json");
            }
            else
            {
                if(!File.Exists("Core/DefaultSettings.json"))
                {
                    Debug.LogError("Missing default settings file!");
                    Program.Game.Exit();
                }

                File.Copy("Core/DefaultSettings.json", "Core/Settings.json");
                json = File.ReadAllText("Core/Settings.json");
                setDisplaySettings = true;
            }

            SettingsJson settingsJson = JsonConvert.DeserializeObject<SettingsJson>(json);
            if (settingsJson == null)
            {
                Debug.LogError("Error reading settings file.");
                Program.Game.Exit();
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
            SettingsJson settingsJson = new SettingsJson(WindowWidth, WindowHeight, WindowTitle, WindowMode,
                Vsync, AntialiasingMode, RefreshRate);
            string json = JsonConvert.SerializeObject(settingsJson, Formatting.Indented);
            File.WriteAllText("Core/Settings.json", json);
        }
    }
}
