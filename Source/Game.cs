﻿using GLFW;
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
using Electron2D.Networking;

namespace Electron2D
{
    public abstract class Game
    {
        public static event Action LateUpdateEvent;

        public Settings Settings { get; private set; }
        public Color BackgroundColor { get; private set; } = Color.Black;
        public Vector4 LinearBackgroundColor { get; private set; } = Vector4.Zero;

        protected Thread PhysicsThread { get; private set; }
        protected CancellationTokenSource PhysicsCancellationToken { get; private set; } = new();
        protected Camera2D StartCamera { get; set; }

        private bool _doFixedUpdate = false;
        private List<IGameClass> _classes = new List<IGameClass>();
        private List<IGameClass> _classesAddQueue = new List<IGameClass>();
        private List<IGameClass> _classesRemoveQueue = new List<IGameClass>();
        private bool _useClassesQueue = false;
        private BlendMode _currentBlendMode = BlendMode.Interpolative;
        private AudioSpatialListener _defaultSpatialListener;

        private void PopClassesQueue()
        {
            for (int i = 0; i < _classesRemoveQueue.Count; i++)
            {
                _classesAddQueue.Remove(_classesRemoveQueue[i]);
            }
            for (int i = 0; i < _classesAddQueue.Count; i++)
            {
                _classes.Add(_classesAddQueue[i]);
            }
            _classesAddQueue.Clear();
            _classesRemoveQueue.Clear();
        }

        public void RegisterGameClass(IGameClass gameClass)
        {
            if (_classes.Contains(gameClass) || _classesAddQueue.Contains(gameClass)) return;
            if(_useClassesQueue)
            {
                _classesAddQueue.Add(gameClass);
            }
            else
            {
                _classes.Add(gameClass);
            }
        }

        public void UnregisterGameClass(IGameClass gameClass)
        {
            if(_useClassesQueue)
            {
                _classesRemoveQueue.Add(gameClass);
            }
            else
            {
                _classesAddQueue.Remove(gameClass);
                _classes.Remove(gameClass);
            }
        }

        public void SetBackgroundColor(Color backgroundColor)
        {
            BackgroundColor = backgroundColor;
            LinearBackgroundColor = new Vector4(MathF.Pow(backgroundColor.R / 255f, 2.2f), MathF.Pow(backgroundColor.G / 255f, 2.2f), MathF.Pow(backgroundColor.B / 255f, 2.2f), backgroundColor.A);
        }

        public void SetBlendingMode(BlendMode blendMode)
        {
            _currentBlendMode = blendMode;
        }

        private void ApplyBlendingMode()
        {
            switch (_currentBlendMode)
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
            _defaultSpatialListener = new AudioSpatialListener(StartCamera.Transform);

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
            glEnable(GL_FRAMEBUFFER_SRGB);
            ApplyBlendingMode();
            // -----------

            #region Splashscreen
            if (ProjectSettings.ShowElectron2DSplashscreen)
            {
                // Displaying splashscreen
                Debug.Log("Displaying splashscreen...");
                Splashscreen.Initialize();
                Texture2D splashscreenTexture = TextureFactory.Load(ResourceManager.GetEngineResourcePath("Textures/Electron2DSplashscreen.png"), true);
                float splashscreenStartTime = (float)Glfw.Time;
                float splashscreenDisplayTime = 4f;
                float fadeTimePercentage = 0.3f;
                float bufferTime = 0.5f;
                float currentTime = -bufferTime;
                bool hasPlayedAudio = false;
                AudioInstance splashscreenAudio = AudioSystem.CreateInstance(ResourceManager.GetEngineResourcePath("Audio/Electron2DRiff.mp3"), volume: 0.3f);
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

            // Initializing physics thread
            PhysicsThread.Start();

            // Initializing built-in network game classes
            NetworkTransform.SetRegisterID(NetworkManager.RegisterNetworkGameClass(NetworkTransform.FactoryMethod));
            NetworkAudioInstance.SetRegisterID(NetworkManager.RegisterNetworkGameClass(NetworkAudioInstance.FactoryMethod));

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

                // Updating
                double goST = Glfw.Time;
                Update();
                ShaderGlobalUniforms.UpdateShaders();
                _useClassesQueue = true;
                foreach (IGameClass gameClass in _classes)
                {
                    gameClass.Update();
                }
                _useClassesQueue = false;
                PopClassesQueue();
                LateUpdateEvent?.Invoke();
                PerformanceTimings.GameObjectMilliseconds = (Glfw.Time - goST) * 1000;
                // --------------------------

                // Physics
                double phyST = Glfw.Time;
                if(_doFixedUpdate)
                {
                    _useClassesQueue = true;
                    foreach (IGameClass gameClass in _classes)
                    {
                        gameClass.FixedUpdate();
                    }
                    _doFixedUpdate = false;
                    _useClassesQueue = false;
                    PopClassesQueue();
                }               
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
                double time = Glfw.Time;
                if (lastTickTime + _physicsTimestep <= time)
                {
                    double delta = time - lastTickTime;
                    Time.FixedDeltaTime = (float)delta;
                    lastTickTime = time;

                    // Do Physics Tick
                    Physics.Step((float)delta, _velocityIterations, _positionIterations);
                    _doFixedUpdate = true;
                }
            }
        }

        private void GLClear()
        {
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT | GL_STENCIL_BUFFER_BIT);
            glClearColor(LinearBackgroundColor.X, LinearBackgroundColor.Y, LinearBackgroundColor.Z, 1);
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
            List<IGameClass> classesCopy = new List<IGameClass>(_classes);
            foreach (IGameClass gameClass in classesCopy)
            {
                gameClass.Dispose();
            }
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