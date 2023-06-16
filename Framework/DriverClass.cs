using Electron2D.Rendering.Display;
using GLFW;
using Electron2D.GameObjects;

namespace Electron2D.Framework
{
    public abstract class DriverClass
    {
        public int currentWindowWidth { get; protected set; }
        public int currentWindowHeight { get; protected set; }
        public string currentWindowTitle { get; protected set; }

        public DriverClass(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle)
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
