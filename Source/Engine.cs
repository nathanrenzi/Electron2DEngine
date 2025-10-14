namespace Electron2D
{
    public static class Engine
    {
        public static Game Game { get; private set; }

        private static bool _initialized = false;

        public static void Initialize(Game game)
        {
            if (_initialized) return;
            _initialized = true;
            Game = game;
        }
    }
}
