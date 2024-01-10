using Electron2D.Build;

namespace Electron2D.Core
{
    public class Program
    {
        public static Game game { get; private set; }
        public static string BuildDate = "";

        [STAThread]
        public static void Main(string[] args)
        {
            GetBuildDate();
            game = new Build.Build(1920, 1080);
            game.Run();
        }

        private static void GetBuildDate()
        {
            BuildDate = File.ReadLines("Build/BuildDate.txt").First();
        }
    }
}