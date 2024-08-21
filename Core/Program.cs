using Electron2D.Build;
using Electron2D.Core.Audio;

namespace Electron2D.Core
{
    public class Program
    {
        public static Game Game { get; private set; }
        public static string BuildDate = "";

        [STAThread]
        public static void Main(string[] args)
        {
            GetBuildDate();
            AudioSystem.Initialize();
            Game = new Build.Build(1920, 1080);
            Game.Run();
        }

        private static void GetBuildDate()
        {
            BuildDate = File.ReadLines("Build/BuildDate.txt").First();
        }
    }
}