using GLFW;
using Electron2D.Core.Rendering;
using static Electron2D.OpenGL.GL;
using System.Numerics;
using Electron2D.Core.Audio;
using Electron2D.Core.Misc;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.UserInterface;
using System.Drawing;
using Electron2D.Core.Rendering.Text;
using Electron2D.Core.PhysicsBox2D;

namespace Electron2D.Core
{
    public abstract class Game
    {
        public static event Action OnStartEvent;
        public static event Action OnUpdateEvent;
        public static event Action OnFixedUpdateEvent;
        public static event Action OnLateUpdateEvent;
        public static readonly float REFERENCE_WINDOW_WIDTH = 1920f;
        public static readonly float REFERENCE_WINDOW_HEIGHT = 1080f;
        public static float WINDOW_SCALE { get { return Program.game.CurrentWindowWidth / REFERENCE_WINDOW_WIDTH; } }

        public int CurrentWindowWidth { get; protected set; }
        public int CurrentWindowHeight { get; protected set; }
        public string CurrentWindowTitle { get; protected set; }
        public bool VsyncEnabled { get; }
        public bool AntialiasingEnabled { get; }

        public static Color BackgroundColor { get; private set; } = Color.Black;

        protected Thread PhysicsThread { get; private set; }
        protected CancellationTokenSource PhysicsCancellationToken { get; private set; } = new();

        protected Camera2D StartCamera { get; set; }

        public Game(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle, float _physicsTimestep = 0.016f, float _physicsGravity = -15f,
            float _physicsLowerBoundX = -100000, float _physicsLowerBoundY = -100000, float _physicsUpperBoundX = 100000, float _physicsUpperBoundY = 100000,
            int _physicsVelocityIterations = 6, int _physicsPositionIterations = 2, bool _vsync = false, bool _antialiasing = true)
        {
            CurrentWindowWidth = _initialWindowWidth;
            CurrentWindowHeight = _initialWindowHeight;
            CurrentWindowTitle = _initialWindowTitle;
            VsyncEnabled = _vsync;
            AntialiasingEnabled = _antialiasing;

            // Starting Physics Thread
            PhysicsThread = new Thread(() => RunPhysicsThread(PhysicsCancellationToken.Token, _physicsTimestep, new Vector2(_physicsLowerBoundX, _physicsLowerBoundY),
                new Vector2(_physicsUpperBoundX, _physicsUpperBoundY), new Vector2(0, _physicsGravity), true, _physicsVelocityIterations, _physicsPositionIterations));
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

            StartCamera = new Camera2D(Vector2.Zero, 1);

            DisplayManager.Instance.CreateWindow(CurrentWindowWidth, CurrentWindowHeight, CurrentWindowTitle, AntialiasingEnabled);
            if(VsyncEnabled)
            {
                // VSYNC ON
                Glfw.SwapInterval(1);
            }
            else
            {
                // VSYNC OFF
                Glfw.SwapInterval(0);
            }
            Input.Initialize();

            // Setup
            glEnable(GL_BLEND);
            glEnable(GL_STENCIL_TEST);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
            // -----------

            // Starting Component Systems
            RigidbodySystem.Start();
            RigidbodySensorSystem.Start();
            TransformSystem.Start();
            MeshRendererSystem.Start();
            TextRendererSystem.Start();
            // -------------------------------
            OnStartEvent?.Invoke();

            // Initializing physics thread
            PhysicsThread.Start();

            ShaderGlobalUniforms.Initialize();
            ShaderGlobalUniforms.RegisterGlobalUniform("lights", LightManager.Instance);
            ShaderGlobalUniforms.RegisterGlobalUniform("time", TimeUniform.Instance);

            GlobalUI.MainCanvas.Initialize();

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

                // Updating -- This could all be parallelized
                double goST = Glfw.Time;
                Update();
                OnUpdateEvent?.Invoke();
                OnLateUpdateEvent?.Invoke();
                PerformanceTimings.GameObjectMilliseconds = (Glfw.Time - goST) * 1000;

                // Updating Component Systems
                RigidbodySystem.Update();
                RigidbodySensorSystem.Update();
                TransformSystem.Update();
                MeshRendererSystem.Update();
                TextRendererSystem.Update();
                // --------------------------

                // Physics
                double phyST = Glfw.Time;
                PerformanceTimings.PhysicsMilliseconds = (Glfw.Time - phyST) * 1000;
                // -------------------------------


                // Rendering
                double rendST = Glfw.Time;
                glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT);
                glClearColor(BackgroundColor.R / 255f, BackgroundColor.G / 255f, BackgroundColor.B / 255f, 1);

                Render();
                RenderLayerManager.RenderAllLayers();

                Glfw.SwapBuffers(DisplayManager.Instance.Window);
                LogErrors();
                PerformanceTimings.RenderMilliseconds = (Glfw.Time - rendST) * 1000;
                // -------------------------------
            }

            OnGameClose();
            PhysicsCancellationToken.Cancel();
            PhysicsThread.Join();
            PhysicsCancellationToken.Dispose();

            Debug.CloseLogFile();
            AudioPlayback.Instance.Dispose();
            DisplayManager.CloseWindow();
        }
        
        private void RunPhysicsThread(CancellationToken _token, double _physicsTimestep, Vector2 _worldLowerBound, Vector2 _worldUpperBound, Vector2 _gravity,
            bool _doSleep, int _velocityIterations = 8, int _positionIterations = 2)
        {
            Physics.Initialize(_worldLowerBound, _worldUpperBound, _gravity, _doSleep);

            double lastTickTime = 0;
            while (!_token.IsCancellationRequested)
            {
                if(lastTickTime + _physicsTimestep <= Glfw.Time)
                {
                    double delta = Glfw.Time - lastTickTime;
                    Time.FixedDeltaTime = (float)delta;
                    lastTickTime = Glfw.Time;

                    TransformSystem.FixedUpdate();
                    MeshRendererSystem.FixedUpdate();
                    TextRendererSystem.FixedUpdate();

                    // Do Physics Tick
                    Physics.Step((float)delta, _velocityIterations, _positionIterations);
                    RigidbodySystem.FixedUpdate();
                    RigidbodySensorSystem.FixedUpdate();
                    OnFixedUpdateEvent?.Invoke();
                }
            }
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
