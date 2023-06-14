using OpenGLTest.GameLoop;

namespace OpenGLTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GameDriver game = new GameDriver(1600, 1200, "Test Game!");
            game.Run();
        }
    }
}