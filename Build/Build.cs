using Electron2D.Core;
using Electron2D.Core.Audio;
using Electron2D.Core.ECS;
using Electron2D.Core.Management;
using Electron2D.Core.Misc;
using Electron2D.Core.Particles;
using Electron2D.Core.PhysicsBox2D;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering.Text;
using Electron2D.Core.UserInterface;
using GLFW;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Build
{
    public class Build : Game
    {
        private TextLabel fpsLabel;
        private UiComponent fpsBackground;
        private int displayFrames;
        private int frames;
        private float lastFrameCountTime;

        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight,
            $"Electron2D Build - {Program.BuildDate}", _vsync: true, _antialiasing: true, _physicsPositionIterations: 4, _physicsVelocityIterations: 8)
        { }

        Entity e;
        protected override void Load()
        {
            // Load Custom Component Systems
            // Ex. ComponentSystem.Start();
            // -----------------------------
            SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));
            InitializeFPSLabel();

            e = new Entity();
            e.AddComponent(new Transform());
            ParticleSystem psys = new ParticleSystem(true, true, true, true, ParticleEmissionShape.Circle, Vector2.UnitY, 360, 100, 1000, _startSizeRange: new Vector2(4, 12),
                _startRotationRange: new Vector2(0, 360), _startAngularVelocityRange: new Vector2(0, 50), _startLifetimeRange: new Vector2(3), _startSpeedRange: new Vector2(600, 700),
                new Gradient(Color.Blue), Material.Create(GlobalShaders.DefaultTexture));
            e.AddComponent(psys);
        }

        protected override void Update()
        {
            e.GetComponent<Transform>().Position = Input.GetMouseWorldPosition();
            CameraMovement();
            CalculateFPS();
        }

        private void CameraMovement()
        {
            Camera2D.Main.Zoom += Input.ScrollDelta;
            Camera2D.Main.Zoom = Math.Clamp(Camera2D.Main.Zoom, 0.2f, 2);

            float moveSpeed = 1000;
            if (Input.GetKey(Keys.W))
            {
                Camera2D.Main.Position += new Vector2(0, moveSpeed * Time.DeltaTime);
            }
            if (Input.GetKey(Keys.A))
            {
                Camera2D.Main.Position += new Vector2(-moveSpeed * Time.DeltaTime, 0);
            }
            if (Input.GetKey(Keys.S))
            {
                Camera2D.Main.Position += new Vector2(0, -moveSpeed * Time.DeltaTime);
            }
            if (Input.GetKey(Keys.D))
            {
                Camera2D.Main.Position += new Vector2(moveSpeed * Time.DeltaTime, 0);
            }
        }

        private void InitializeFPSLabel()
        {
            fpsLabel = new TextLabel("FPS: 0", "Build/Resources/Fonts/OpenSans.ttf",
                30, Color.White, Color.White, new Vector2(130, 30), TextAlignment.Left, TextAlignment.Center,
                TextAlignmentMode.Geometry, TextOverflowMode.Disabled, _uiRenderLayer: 11);
            Material m = Material.Create(GlobalShaders.DefaultTexture, Color.FromArgb(60, 0, 0, 0), ResourceManager.Instance.LoadTexture("Build/Resources/Textures/white_circle.png"));
            fpsBackground = new SlicedUiComponent(m, 160, 40, 100, 100, 100, 100, 200, 0.2f);

            fpsLabel.Anchor = new Vector2(-1, 1);
            fpsLabel.Transform.Position = new Vector2((-1920 / 2) + 23, (1080 / 2) - 20);
            fpsBackground.Transform.Position = new Vector2((-1920 / 2) + 70 + 20, (1080 / 2) - 15 - 20);
        }

        private void CalculateFPS()
        {
            frames++;
            if (Time.GameTime - lastFrameCountTime >= 1)
            {
                lastFrameCountTime = Time.GameTime;
                displayFrames = frames;
                frames = 0;
            }

            fpsLabel.Text = $"FPS: {displayFrames}";
        }

        protected unsafe override void Render()
        {

        }
    }
}