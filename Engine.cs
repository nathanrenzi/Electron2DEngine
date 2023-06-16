using OpenGLTest.Game;

namespace Electron2D
{
    /// <summary>
    /// The entry point of the Electron2D engine.
    /// </summary>
    public class Engine
    {
        public static MainDriver mainDriver { get; private set; }

        public static void Main(string[] args)
        {
            mainDriver = new MainDriver(1920, 1080, "Test Game!");
            mainDriver.Run();
        }
    }
}