using Electron2D.Core;
using Electron2D.Core.Management.Textures;
using Electron2D.Core.Management;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering.Text;
using Electron2D.Core.UI;
using Electron2D.Core.UserInterface;
using GLFW;
using System.Drawing;
using System.Numerics;
using Electron2D.Core.Misc;
using Electron2D.Core.ECS;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

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
            SetBackgroundColor(System.Drawing.Color.FromArgb(255, 80, 80, 80));
            InitializeFPSLabel();

            s = new Sprite(Material.Create(GlobalShaders.DefaultTextureArray, System.Drawing.Color.Navy));

            Box2DInit();
        }

        private Vec2 gravity = new Vec2(0.0f, -10.0f);
        private World world;
        private Body dynamicBody;

        private void Box2DInit()
        {
            // Initializing the world
            AABB worldAABB = new AABB();
            worldAABB.LowerBound.Set(-100, -100);
            worldAABB.UpperBound.Set(100, 100);
            world = new World(worldAABB, gravity, false);

            // Creating ground body
            BodyDef groundBodyDef = new BodyDef();
            groundBodyDef.Position.Set(0.0f, -10.0f);
            Body groundBody = world.CreateBody(groundBodyDef);

            // Defining ground polygon
            PolygonDef groundShapeDef = new PolygonDef();
            groundShapeDef.SetAsBox(50, 10);

            // Creating ground fixture
            Fixture groundFixture = groundBody.CreateFixture(groundShapeDef);

            // ------------------------------------

            // Creating dynamic body
            BodyDef dBodyDef = new BodyDef();
            dBodyDef.Position.Set(0.0f, 4.0f);
            dynamicBody = world.CreateBody(dBodyDef);

            // Defining dynamic polygon
            PolygonDef dynamicShapeDef = new PolygonDef();
            dynamicShapeDef.SetAsBox(1, 1);
            dynamicShapeDef.Density = 1;
            dynamicShapeDef.Friction = 0.3f;

            // Creating dynamic fixture
            Fixture dynamicFixture = dynamicBody.CreateFixture(dynamicShapeDef);
        }

        protected override void Update()
        {
            CameraMovement();
            CalculateFPS();
        }

        private void CameraMovement()
        {
            Camera2D.main.zoom += Input.scrollDelta;
            Camera2D.main.zoom = System.Math.Clamp(Camera2D.main.zoom, 0.2f, 2);

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
                30, System.Drawing.Color.White, System.Drawing.Color.White, new Vector2(130, 30), TextAlignment.Left, TextAlignment.Center,
                TextAlignmentMode.Geometry, TextOverflowMode.Disabled, _uiRenderLayer: 11);
            Material m = Material.Create(GlobalShaders.DefaultTexture, System.Drawing.Color.FromArgb(60, 0, 0, 0), ResourceManager.Instance.LoadTexture("Build/Resources/Textures/white_circle.png"));
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
