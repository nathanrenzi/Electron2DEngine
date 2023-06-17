using Electron2D.Build;

namespace Electron2D.Core
{
    public class Program
    {
        public static Game game { get; private set; }

        public static void Main(string[] args)
        {
            game = new MainGame(1920, 1080, "Test Game!");
            game.Run();
        }
    }
}