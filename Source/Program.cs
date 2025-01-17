using Electron2D.Audio;

namespace Electron2D
{
    public class Program
    {
        public static Game Game { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            AudioSystem.Initialize();
            Game = new Build();
            Game.Run();
        }
    }
}