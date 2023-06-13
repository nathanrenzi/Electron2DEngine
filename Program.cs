using OpenGLTest.GameLoop;

namespace OpenGLTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TestGame game = new TestGame(1600, 1200, "Test Game!");
            game.Run();
        }
    }
}