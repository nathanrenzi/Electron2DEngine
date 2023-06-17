using GLFW;
using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering;
using static Electron2D.OpenGL.GL;

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
            Camera2D camera = new Camera2D(DisplayManager.windowSize / 2, 1);

            DisplayManager.CreateWindow(currentWindowWidth, currentWindowHeight, currentWindowTitle);

            // Enabling transparent textures
            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            // -----------

            LoadContent();

            while(!Glfw.WindowShouldClose(DisplayManager.window))
            {
                Time.deltaTime = (float)Glfw.Time - Time.totalElapsedSeconds;
                Time.totalElapsedSeconds = (float)Glfw.Time;
                
                // Updating
                GameObjectManager.UpdateGameObjects();
                Update();
                // -------------------------------


                // Input
                Glfw.PollEvents();
                // -------------------------------


                // Rendering
                glClear(GL_COLOR_BUFFER_BIT);
                glClearColor(0.2f, 0.2f, 0.2f, 1);

                GameObjectManager.RenderGameObjects();
                Render();

                Glfw.SwapBuffers(DisplayManager.window);
                // -------------------------------
            }

            DisplayManager.CloseWindow();
        }

        protected abstract void Initialize();
        protected abstract void LoadContent();

        protected abstract void Update();
        protected abstract void Render();
    }
}
