using Electron2D.Audio;

namespace Electron2D
{
    public class Program
    {
        public static Game Game { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            Settings settings = Settings.LoadSettingsFile();
            AudioSystem.Initialize(settings.AudioMasterVolume);
            Game = new Build();
            Game.Run(settings);
        }
    }
}