using Electron2D.Build;
using Electron2D.Core.Audio;

namespace Electron2D.Core
{
    public class Program
    {
        public static Game game { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            AudioPlayback.Initialize(1f); // Must be called in main, and Main must have [STAThread] attribute for ASIO
            game = new MainGame(1920, 1080, "Test Game!");
            game.Run();
        }
    }
}