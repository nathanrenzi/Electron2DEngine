using OpenGLTest.Rendering.Display;
using GLFW;
using OpenGLTest.GameObjects;

namespace OpenGLTest.GameLoop
{
    public abstract class BaseGame
    {
        protected int initialWindowWidth;
        protected int initialWindowHeight;
        protected string initialWindowTitle;

        public BaseGame(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle)
        {
            initialWindowWidth = _initialWindowWidth;
            initialWindowHeight = _initialWindowHeight;
            initialWindowTitle = _initialWindowTitle;
        }

        public void Run()
        {
            Initialize();

            DisplayManager.CreateWindow(initialWindowWidth, initialWindowHeight, initialWindowTitle);

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
