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

namespace Electron2D.Build
{
    public class Build : Game
    {
        private TextLabel fpsLabel;
        private UiComponent fpsBackground;
        private int displayFrames;
        private int frames;
        private float lastFrameCountTime;
        private SliderSimple slider;
        private Tilemap tilemap;
        private List<Light> lights = new List<Light>();
        private List<float> lradius = new List<float>();

        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight,
            $"Electron2D Build - {Program.BuildDate}", _vsync: false, _antialiasing: true) { }

        protected override void Load()
        {
            SetBackgroundColor(Color.FromArgb(255, 10, 10, 10));
            
            // Load Custom Component Systems
            // Ex. ComponentSystem.Start();
            // -----------------------------

            InitializeFPSLabel();

            TextLabel label = new TextLabel("Light Controller", "Build/Resources/Fonts/OpenSans.ttf", 15, Color.White, Color.White, new Vector2(200, 50),
                TextAlignment.Center, TextAlignment.Center, TextAlignmentMode.Geometry);
            label.Transform.Position = new Vector2(0, 20);

            slider = new SliderSimple("414643".HexToColor(255), "9BB6A1".HexToColor(255), Color.White, _value: 0, _minValue: 0, _maxValue: 10, 200, 6, 10, 20);
            Panel bgPanel = new Panel(Color.FromArgb(60, 0, 0, 0), -1, 250, 80);
            bgPanel.Transform.Position = new Vector2(0, 10);


            // Tilemap Setup
            Texture2D tex1 = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/tiles1.png");
            Texture2D tex2 = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/tilesNormal1.png", true);
            Spritesheets.Add(tex1, 2, 2);

            Shader diffuseShader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultLit.glsl"),
                _globalUniformTags: new string[] { "lights" });

            // Creating tiles
            int size = 100;
            int[] tiles = new int[size * size];
            Random random = new Random();
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = random.Next(0, 2) == 0 ? 0 : (random.Next(0, 6) > 0 ? 1 : 2);
            }
            //---------------

            int tilePixelSize = 100;
            tilemap = new Tilemap(Material.Create(diffuseShader, _mainTexture: tex1, _normalTexture: tex2, _useLinearFiltering: false, _normalScale: 1),
                new TileData[] { new TileData("Grass1", 0, 1), new TileData("Grass2", 1, 1), new TileData("Pebble", 0, 0) }, tilePixelSize, size, size, tiles);

            tilemap.GetComponent<Transform>().Position = new Vector2(-1920/2f, -1080/2f);


            for (int i = 0; i < LightManager.MAX_POINT_LIGHTS; i++)
            {
                Light l = new Light(Color.White, random.Next(1, 8) * 100, random.Next(1, 3), Light.LightType.Point, 2);
                l.GetComponent<Transform>().Position = new Vector2(random.Next(0, size * tilePixelSize), random.Next(0, size * tilePixelSize));
                lights.Add(l);
                lradius.Add(l.Radius);
            }
        }

        protected override void Update()
        {
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].Intensity = Easing.EaseInOutSine(slider.Value01);
                lights[i].Radius = slider.Value01 * lradius[i];
            }
            CameraMovement();
            CalculateFPS();
        }

        private void CameraMovement()
        {
            Camera2D.main.zoom += Input.scrollDelta;
            Camera2D.main.zoom = Math.Clamp(Camera2D.main.zoom, 0.5f, 2);

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
            fpsBackground = new Panel(Color.Black, 10, 140, 30);

            fpsLabel.Anchor = new Vector2(-1, 1);
            fpsLabel.Transform.Position = new Vector2(-1920 / 2, 1080 / 2);
            fpsBackground.Anchor = new Vector2(-1, 1);
            fpsBackground.Transform.Position = new Vector2(-1920 / 2, 1080 / 2);
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
