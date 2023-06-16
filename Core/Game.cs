using GLFW;
using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering;

namespace Electron2D.Core
{
    public abstract class Game
    {
        public int currentWindowWidth { get; protected set; }
        public int currentWindowHeight { get; protected set; }
        public string currentWindowTitle { get; protected set; }

        public Game(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle)
        {
            currentWindowWidth = _initialWindowWidth;
            currentWindowHeight = _initialWindowHeight;
            currentWindowTitle = _initialWindowTitle;
        }

        public void Run()
        {
            Initialize();

            DisplayManager.CreateWindow(currentWindowWidth, currentWindowHeight, currentWindowTitle);

            LoadContent();

            while(!Glfw.WindowShouldClose(DisplayManager.window))
            {
                Time.deltaTime = (float)Glfw.Time - Time.totalElapsedSeconds;
                Time.totalElapsedSeconds = (float)Glfw.Time;
                
                Update();

                Glfw.PollEvents();
                
                Render();
            }

            DisplayManager.CloseWindow();
        }

        protected abstract void Initialize();
        protected abstract void LoadContent();

        protected abstract void Update();
        protected abstract void Render();
    }
}
