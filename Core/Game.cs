using GLFW;
using Electron2D.Core.Rendering;
using static Electron2D.OpenGL.GL;
using System.Numerics;
using Electron2D.Core.Audio;
using Electron2D.Core.Misc;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.UserInterface;
using System.Drawing;

namespace Electron2D.Core
{
    public abstract class Game
    {
        public static event Action OnStartEvent;
        public static event Action OnUpdateEvent;
        public static event Action OnLateUpdateEvent;
        public static readonly float REFERENCE_WINDOW_WIDTH = 1920f;
        public static readonly float REFERENCE_WINDOW_HEIGHT = 1080f;
        public static float WINDOW_SCALE { get { return Program.game.CurrentWindowWidth / REFERENCE_WINDOW_WIDTH; } }

        public int CurrentWindowWidth { get; protected set; }
        public int CurrentWindowHeight { get; protected set; }
        public string CurrentWindowTitle { get; protected set; }

        public static Color BackgroundColor { get; private set; } = Color.Black;

        protected Camera2D startCamera;

        public Game(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle)
        {
            CurrentWindowWidth = _initialWindowWidth;
            CurrentWindowHeight = _initialWindowHeight;
            CurrentWindowTitle = _initialWindowTitle;
        }

        public void SetBackgroundColor(Color _backgroundColor)
        {
            BackgroundColor = _backgroundColor;
        }

        public void Run()
        {
            Debug.OpenLogFile();
            Debug.Log("Starting initialization...");
            Initialize();
            startCamera = new Camera2D(Vector2.Zero, 1);

            DisplayManager.Instance.CreateWindow(CurrentWindowWidth, CurrentWindowHeight, CurrentWindowTitle);
            Glfw.SwapInterval(1); // 0 - VSYNC is off, 1 is on
            Input.Initialize();

            // Setup
            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            glEnable(GL_FRAMEBUFFER_SRGB); // Gamma-corrects lighting and improves overall scene
            //glPixelStorei(GL_UNPACK_ALIGNMENT, 1); // Disable byte-alignment restriction - used for text rendering https://learnopengl.com/In-Practice/Text-Rendering
            // -----------

            // Starting Entity Systems
            TransformSystem.Start();
            MeshRendererSystem.Start();
            LightSystem.Start();
            // -----------------
            OnStartEvent?.Invoke();

            ShaderGlobalUniforms.Initialize();
            ShaderGlobalUniforms.RegisterGlobalUniform("lights", LightManager.Instance);
            ShaderGlobalUniforms.RegisterGlobalUniform("time", TimeUniform.Instance);

            UiMaster.Display.Initialize();

            GC.Collect();

            Debug.Log("Initialization complete");

            Load();

            while (!Glfw.WindowShouldClose(DisplayManager.Instance.Window))
            {
                Time.DeltaTime = (float)Glfw.Time - Time.TotalElapsedSeconds;
                Time.TotalElapsedSeconds = (float)Glfw.Time;
                PerformanceTimings.FramesPerSecond = 1 / Time.DeltaTime;

                // Input
                Input.ProcessInput();
                // -------------------------------

                // Updating
                double goST = Glfw.Time;
                Update();
                OnUpdateEvent?.Invoke();
                TransformSystem.Update();
                MeshRendererSystem.Update();
                LightSystem.Update();
                OnLateUpdateEvent?.Invoke();
                PerformanceTimings.GameObjectMilliseconds = (Glfw.Time - goST) * 1000;
                // -------------------------------


                // Physics
                double phyST = Glfw.Time;
                PerformanceTimings.PhysicsMilliseconds = (Glfw.Time - phyST) * 1000;
                // -------------------------------


                // Rendering
                double rendST = Glfw.Time;
                glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
                glClearColor(BackgroundColor.R / 255f, BackgroundColor.G / 255f, BackgroundColor.B / 255f, 1);

                Render();
                RenderLayerManager.RenderAllLayers();

                Glfw.SwapBuffers(DisplayManager.Instance.Window);
                LogErrors();
                PerformanceTimings.RenderMilliseconds = (Glfw.Time - rendST) * 1000;
                // -------------------------------
            }

            OnGameClose();

            Debug.CloseLogFile();
            AudioPlayback.Instance.Dispose();
            DisplayManager.CloseWindow();
        }
        
        private void LogErrors()
        {
            int errorCode = GetError();
            if (errorCode != GL_NO_ERROR)
                Debug.LogError($"OPENGL {errorCode}");

            string description;
            ErrorCode code = Glfw.GetError(out description);
            while (code != ErrorCode.None)
            {
                code = Glfw.GetError(out description);
                Debug.LogError($"GLFW {description} | {code}");
            }
        }

        protected virtual void Initialize() { }      // This is ran when the Game is first initialized
        protected virtual void Load() { }            // This is ran when the Game is ready to load content
        protected virtual void Update() { }          // This is ran every frame before Render()
        protected virtual void Render() { }          // This is ran every frame after Update()
        protected virtual void OnGameClose() { }     // This is ran when the game is closing
    }
}
