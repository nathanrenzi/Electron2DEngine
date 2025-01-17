using Electron2D.Core.Audio;

namespace Electron2D.Core
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