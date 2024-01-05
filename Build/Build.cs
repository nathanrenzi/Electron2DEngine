using Electron2D.Core;
using Electron2D.Core.Management;
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
        private Sprite s;

        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight,
            $"Electron2D Build - {Program.BuildDate}", _vsync: false, _antialiasing: false) { }

        protected override void Load()
        {
            // Load Custom Component Systems
            // Ex. ComponentSystem.Start();
            // -----------------------------

            SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));
            InitializeFPSLabel();
            
            s = new Sprite(Material.Create(GlobalShaders.DefaultTexture, Color.Navy));
            RigidbodyDynamicDef df = new RigidbodyDynamicDef()
            {
                Velocity = Vector2.UnitY * 10,
                Shape = RigidbodyShape.Box
            };
            s.AddComponent(Rigidbody.CreateDynamic(df));

            RigidbodyStaticDef sf = new RigidbodyStaticDef();
            Sprite b = new Sprite(Material.Create(GlobalShaders.DefaultTexture, Color.White), 0, 6, 6);
            b.Transform.Position = new Vector2(40, -250f);
            b.AddComponent(Rigidbody.CreateStatic(sf));

            Sprite a = new Sprite(Material.Create(GlobalShaders.DefaultTexture, Color.White), 0, 500, 30);
            a.Transform.Position = new Vector2(0, -450f);
            a.AddComponent(Rigidbody.CreateStatic(sf));

            Sprite f = new Sprite(Material.Create(GlobalShaders.DefaultTexture, Color.FromArgb(50, 255, 255, 255)), 0, 30, 30);
            f.Transform.Position = new Vector2(-50, -350);
            RigidbodySensor sensor = new RigidbodySensor(new Vector2(30), _shape: ColliderSensorShape.Box);
            f.AddComponent(sensor);
            sensor.OnBeginContact += (rb) => Debug.Log("Entered sensor");
            sensor.OnEndContact += (rb) => Debug.Log("Exited sensor");
        }

        protected override void Update()
        {
            CameraMovement();
            CalculateFPS();
        }

        private void CameraMovement()
        {
            Camera2D.main.zoom += Input.scrollDelta;
            Camera2D.main.zoom = Math.Clamp(Camera2D.main.zoom, 0.2f, 2);

            float moveSpeed = 1000;
            if (Input.GetKey(Keys.W))
            {
                Camera2D.main.position += new Vector2(0, moveSpeed * Time.DeltaTime);
            }
            if (Input.GetKey(Keys.A))
            {
                Camera2D.main.position += new Vector2(-moveSpeed * Time.DeltaTime, 0);
            }
            if (Input.GetKey(Keys.S))
            {
                Camera2D.main.position += new Vector2(0, -moveSpeed * Time.DeltaTime);
            }
            if (Input.GetKey(Keys.D))
            {
                Camera2D.main.position += new Vector2(moveSpeed * Time.DeltaTime, 0);
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
            if (Time.TotalElapsedSeconds - lastFrameCountTime >= 1)
            {
                lastFrameCountTime = Time.TotalElapsedSeconds;
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
