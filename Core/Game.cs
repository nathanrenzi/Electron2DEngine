using GLFW;
using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering;
using static Electron2D.OpenGL.GL;
using System.Numerics;
using Electron2D.Core.Audio;
using Electron2D.Core.Physics;
using Electron2D.Core.Misc;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.UserInterface;

namespace Electron2D.Core
{
    public abstract class Game
    {
        public static event Action onStartEvent;
        public static event Action onUpdateEvent;
        public static event Action onLateUpdateEvent;
        public static readonly float REFERENCE_WINDOW_WIDTH = 1920f;
        public static readonly float REFERENCE_WINDOW_HEIGHT = 1080f;
        public static float WINDOW_SCALE { get { return Program.game.currentWindowWidth / REFERENCE_WINDOW_WIDTH; } }

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

            // Setup
            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            // -----------

            Start();
            onStartEvent?.Invoke();
            GameObjectManager.StartGameObjects();
            UiMaster.display.Initialize();

            while (!Glfw.WindowShouldClose(DisplayManager.Instance.window))
            {
                Time.deltaTime = (float)Glfw.Time - Time.totalElapsedSeconds;
                Time.totalElapsedSeconds = (float)Glfw.Time;
                PerformanceTimings.framesPerSecond = 1 / Time.deltaTime;

                // Input
                Input.ProcessInput();
                // -------------------------------

                // Updating
                double goST = Glfw.Time;
                Update();
                onUpdateEvent?.Invoke();
                GameObjectManager.UpdateGameObjects();
                onLateUpdateEvent?.Invoke();
                PerformanceTimings.gameObjectMilliseconds = (Glfw.Time - goST) * 1000;
                // -------------------------------


                // Physics
                double phyST = Glfw.Time;
                VerletWorld.Step();
                PerformanceTimings.physicsMilliseconds = (Glfw.Time - phyST) * 1000;
                // -------------------------------


                // Rendering
                double rendST = Glfw.Time;
                glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
                glClearColor(0.2f, 0.2f, 0.2f, 1);

                Render();
                RenderLayerManager.RenderAllLayers();

                Glfw.SwapBuffers(DisplayManager.Instance.window);
                PerformanceTimings.renderMilliseconds = (Glfw.Time - rendST) * 1000;
                // -------------------------------
            }

            AudioPlayback.Instance.Dispose();
            DisplayManager.CloseWindow();
        }

        protected abstract void Initialize();   // This is ran when the Game is first initialized
        protected abstract void Start();        // This is ran when the Game is ready to load content
        protected abstract void Update();       // This is ran every frame before Render()
        protected abstract void Render();       // This is ran every frame after Update()
    }
}
