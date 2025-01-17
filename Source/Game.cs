using GLFW;
using Electron2D.Rendering;
using static Electron2D.OpenGL.GL;
using System.Numerics;
using Electron2D.Misc;
using Electron2D.UserInterface;
using System.Drawing;
using Electron2D.PhysicsBox2D;
using Electron2D.Audio;
using Electron2D.Management;
using Electron2D.Rendering.PostProcessing;

namespace Electron2D
{
    public abstract class Game
    {
        public static event Action OnStartEvent;
        public static event Action OnUpdateEvent;
        public static event Action OnFixedUpdateEvent;
        public static event Action OnLateUpdateEvent;
        public Settings Settings { get; private set; }
        public static Color BackgroundColor { get; private set; } = Color.Black;

        protected Thread PhysicsThread { get; private set; }
        protected CancellationTokenSource PhysicsCancellationToken { get; private set; } = new();
        protected Camera2D StartCamera { get; set; }

        private BlendMode currentBlendMode = BlendMode.Interpolative;

        public void SetBackgroundColor(Color _backgroundColor)
        {
            BackgroundColor = _backgroundColor;
        }

        public void SetBlendingMode(BlendMode _blendMode)
        {
            currentBlendMode = _blendMode;
        }

        private void ApplyBlendingMode()
        {
            switch (currentBlendMode)
            {
                case BlendMode.Interpolative:
                    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
                    break;
                case BlendMode.Additive:
                    glBlendFunc(GL_ONE, GL_ONE);
                    break;
                case BlendMode.Multiplicative:
                    glBlendFunc(GL_DST_COLOR, GL_ZERO);
                    break;
                default:
                    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
                    break;
            }
        }

        public void Run(Settings settings)
        {
            Settings = settings;
            Debug.OpenLogFile();
            Debug.Log("Starting initialization...");
            Display.Initialize();
            Cursor.Initialize();

            // Starting Physics Thread
            PhysicsThread = new Thread(() => RunPhysicsThread(PhysicsCancellationToken.Token, ProjectSettings.PhysicsTimestep,
                new Vector2(0, ProjectSettings.PhysicsGravity), true, ProjectSettings.PhysicsVelocityIterations,
                ProjectSettings.PhysicsPositionIterations));

            Initialize();

            StartCamera = new Camera2D(Vector2.Zero, 1);
            StartCamera.AddComponent(new AudioSpatialListener());

            Display.CreateWindow(Settings.WindowWidth, Settings.WindowHeight, ProjectSettings.WindowTitle);
            if(Settings.Vsync)
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
            ApplyBlendingMode();
            // -----------

            #region Splashscreen
            if (ProjectSettings.ShowElectron2DSplashscreen)
            {
                // Displaying splashscreen
                Debug.Log("Displaying splashscreen...");
                Splashscreen.Initialize();
                Texture2D splashscreenTexture = TextureFactory.Load("Resources/Built-In/Textures/Electron2DSplashscreen.png", true);
                float splashscreenStartTime = (float)Glfw.Time;
                float splashscreenDisplayTime = 4f;
                float fadeTimePercentage = 0.3f;
                float bufferTime = 0.5f;
                float currentTime = -bufferTime;
                bool hasPlayedAudio = false;
                AudioInstance splashscreenAudio = AudioSystem.CreateInstance("Resources/Built-In/Audio/Electron2DRiff.mp3", _volume: 0.3f);
                while (!Glfw.WindowShouldClose(Display.Window) && (currentTime - bufferTime) < splashscreenDisplayTime)
                {
                    Input.ProcessInput(); // Letting the window know the program is responding

                    float t = MathEx.Clamp01((currentTime - bufferTime) / splashscreenDisplayTime);
                    float e;
                    if (t <= fadeTimePercentage)
                    {
                        e = Easing.EaseInOutSine(t / fadeTimePercentage);
                    }
                    else if (t >= 1 - fadeTimePercentage)
                    {
                        e = Easing.EaseInOutSine((1 - t) / fadeTimePercentage);
                    }
                    else
                    {
                        e = 1;
                    }

                    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT);
                    glClearColor(0, 0, 0, 1);
                    Splashscreen.Render(splashscreenTexture, (int)(e * 255));
                    if (t > fadeTimePercentage / 2f && !hasPlayedAudio)
                    {
                        hasPlayedAudio = true;
                        splashscreenAudio.Play();
                    }
                    Glfw.SwapBuffers(Display.Window);

                    currentTime = (float)Glfw.Time - splashscreenStartTime;
                }
                splashscreenAudio?.Dispose();
                Splashscreen.Dispose();
                splashscreenTexture.Dispose();
                Debug.Log("Splashscreen ended");
            }
            #endregion

            // Starting Component Systems
            RigidbodySystem.Start();
            RigidbodySensorSystem.Start();
            ParticleSystemBaseSystem.Start();
            TransformSystem.Start();
            MeshRendererSystem.Start();
            AudioSpatializerSystem.Start();
            // -------------------------------
            OnStartEvent?.Invoke();

            // Initializing physics thread
            PhysicsThread.Start();

            ShaderGlobalUniforms.Initialize();
            ShaderGlobalUniforms.RegisterGlobalUniform("lights", LightManager.Instance);
            ShaderGlobalUniforms.RegisterGlobalUniform("time", TimeUniform.Instance);

            GlobalUI.MainCanvas.Initialize();
            PostProcessor.Instance.Initialize();
            GC.Collect();

            Debug.Log("Initialization complete");
            Debug.Log("#################### GAME STARTED ####################", ConsoleColor.DarkGreen);

            Load();
            // Rendering before the game loop prevents a black screen when the window is opened
            GLClear();
            RenderCall();

            while (!Glfw.WindowShouldClose(Display.Window))
            {
                Time.DeltaTime = (float)Glfw.Time - Time.GameTime;
                Time.GameTime = (float)Glfw.Time;
                PerformanceTimings.FramesPerSecond = 1 / Time.DeltaTime;

                // Input
                Input.ProcessInput();
                // -----------------------

                // Updating -- This could all be parallelized
                double goST = Glfw.Time;
                Update();
                OnUpdateEvent?.Invoke();
                OnLateUpdateEvent?.Invoke();
                PerformanceTimings.GameObjectMilliseconds = (Glfw.Time - goST) * 1000;

                // Updating Component Systems
                RigidbodySystem.Update();
                RigidbodySensorSystem.Update();
                ParticleSystemBaseSystem.Update();
                TransformSystem.Update();
                MeshRendererSystem.Update();
                AudioSpatializerSystem.Update();
                // --------------------------

                // Physics
                double phyST = Glfw.Time;
                PerformanceTimings.PhysicsMilliseconds = (Glfw.Time - phyST) * 1000;
                // -------------------------------


                // Rendering
                double rendST = Glfw.Time;
                ApplyBlendingMode();
                GLClear();
                double ppST = Glfw.Time;
                PostProcessor.Instance.BeforeGameRender();
                RenderCall();
                PostProcessor.Instance.AfterGameRender();
                PostProcessor.Instance.Render();
                PerformanceTimings.PostProcessingMilliseconds = (Glfw.Time - ppST) * 1000;
                RenderLayerManager.RenderAllLayersIgnorePostProcessing();

                Glfw.SwapBuffers(Display.Window);
                if(ProjectSettings.GraphicsErrorCheckingEnabled) LogErrors();
                PerformanceTimings.RenderMilliseconds = (Glfw.Time - rendST) * 1000;
                // -------------------------------
            }

            Exit(false);
        }

        private void RunPhysicsThread(CancellationToken _token, double _physicsTimestep, Vector2 _gravity,
            bool _doSleep, int _velocityIterations = 8, int _positionIterations = 2)
        {
            Physics.Initialize(_gravity, _doSleep);

            double lastTickTime = 0;
            while (!_token.IsCancellationRequested)
            {
                if (lastTickTime + _physicsTimestep <= Glfw.Time)
                {
                    double delta = Glfw.Time - lastTickTime;
                    Time.FixedDeltaTime = (float)delta;
                    lastTickTime = Glfw.Time;

                    AudioSpatializerSystem.FixedUpdate();
                    TransformSystem.FixedUpdate();
                    MeshRendererSystem.FixedUpdate();

                    // Do Physics Tick
                    Physics.Step((float)delta, _velocityIterations, _positionIterations);
                    RigidbodySystem.FixedUpdate();
                    RigidbodySensorSystem.FixedUpdate();
                    OnFixedUpdateEvent?.Invoke();
                }
            }
        }

        private void GLClear()
        {
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT);
            glClearColor(BackgroundColor.R / 255f, BackgroundColor.G / 255f, BackgroundColor.B / 255f, 1);
        }

        private void RenderCall()
        {
            Render();
            RenderLayerManager.RenderAllLayers();
        }

        private void LogErrors()
        {
            int errorCode = GetError();
            if (errorCode != GL_NO_ERROR)
                Debug.LogError($"OPENGL {errorCode}");

            string description;
            ErrorCode code = Glfw.GetError(out description);
            if(code != ErrorCode.None)
            {
                Debug.LogError($"GLFW {description} | {code}");
            }
            while (code != ErrorCode.None)
            {
                code = Glfw.GetError(out description);
                Debug.LogError($"GLFW {description} | {code}");
            }
        }

        /// <summary>
        /// Exits the game.
        /// </summary>
        public void Exit(bool terminateGlfw = true)
        {
            Display.DestroyWindow();
            OnGameClose();
            PhysicsCancellationToken.Cancel();
            PhysicsThread.Join();
            PhysicsCancellationToken.Dispose();
            AudioSystem.Dispose();
            Debug.CloseLogFile();
            if (terminateGlfw) Glfw.Terminate();
        }

        protected virtual void Initialize() { }      // This is ran when the game is first initialized
        protected virtual void Load() { }            // This is ran when the game is ready to load content
        protected virtual void Update() { }          // This is ran every frame
        protected virtual void Render() { }          // This is ran every frame right before rendering
        protected virtual void OnGameClose() { }     // This is ran when the game is closing
    }
}