using OpenGLTest.GameLoop;

namespace OpenGLTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TestGame game = new TestGame(800, 600, "Test Game!");
            game.Run();
        }
    }
}