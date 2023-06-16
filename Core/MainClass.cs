using Electron2D.Build;

namespace Electron2D.Core
{
    /// <summary>
    /// The entry point of the Electron2D engine.
    /// </summary>
    public class MainClass
    {
        public static Game game { get; private set; }

        public static void Main(string[] args)
        {
            game = new TestGame(1920, 1080, "Test Game!");
            game.Run();
        }
    }
}