using GLFW;
using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering;
using static Electron2D.OpenGL.GL;
using System.Numerics;

namespace Electron2D.Core
{
    public abstract class Game
    {
        public int currentWindowWidth { get; protected set; }
        public int currentWindowHeight { get; protected set; }
        public string currentWindowTitle { get; protected set; }

        protected Camera2D startCamera;

        public Game(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle)
        {
            currentWindowWidth = _initialWindowWidth;
            currentWindowHeight = _initialWindowHeight;
            currentWindowTitle = _initialWindowTitle;
        }

        public void Run()
        {
            Initialize();
            startCamera = new Camera2D(Vector2.Zero, 1);

            DisplayManager.Instance.CreateWindow(currentWindowWidth, currentWindowHeight, currentWindowTitle);
            Input.Initialize();

            // Enabling transparent textures
            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            // -----------

            LoadContent();
            GameObjectManager.StartGameObjects();

            while (!Glfw.WindowShouldClose(DisplayManager.Instance.window))
            {
                Time.deltaTime = (float)Glfw.Time - Time.totalElapsedSeconds;
                Time.totalElapsedSeconds = (float)Glfw.Time;

                // Updating
                Update();
                GameObjectManager.UpdateGameObjects();
                // -------------------------------


                // Input
                Input.ProcessInput();
                // -------------------------------


                // Rendering
                glClear(GL_COLOR_BUFFER_BIT);
                glClearColor(0.2f, 0.2f, 0.2f, 1);

                Render();
                GameObjectManager.RenderGameObjects();

                Glfw.SwapBuffers(DisplayManager.Instance.window);
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
