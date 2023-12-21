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
        private Sprite s;

        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight,
            $"Electron2D Build - {Program.BuildDate}", _vsync: false, _antialiasing: false) { }

        protected override void Load()
        {
            SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));

            // Load Custom Component Systems
            // Ex. ComponentSystem.Start();
            // -----------------------------

            InitializeFPSLabel();

            Texture2DArray tex = ResourceManager.Instance.LoadTextureArray("Build/Resources/Textures/test_array.png", 6);
            s = new Sprite(Material.Create(GlobalShaders.DefaultTextureArray, tex), 12, 160, 160);

            //slider = new SliderSimple("414643".HexToColor(255), "9BB6A1".HexToColor(255), Color.White, _value: 0, _minValue: 0, _maxValue: 10,
            //    _sizeX: 200, _sliderHeight: 10, _backgroundHeight: 6, _handleSize: 10, _handlePadding: 5);

            //#region Tilemap
            //// Tilemap Setup
            //Texture2D tex1 = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/FantasyTileset/TX Tileset Grass.png");
            ////Texture2D tex2 = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/tilesNormal1.png", true);
            //Spritesheets.Add(tex1, 8, 8);

            //Shader diffuseShader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultLit.glsl"), _globalUniformTags: new string[] {"lights"});

            //LightManager.AmbientColor = Color.FromArgb(255, 150, 150, 150);

            //// Creating tiles
            //int size = 500;
            //int tileTypes = 62;
            //int[] tiles = new int[size * size];
            //Random random = new Random();
            //for (int i = 0; i < tiles.Length; i++)
            //{
            //    tiles[i] = random.Next(0, tileTypes);
            //}
            ////---------------

            //List<TileData> tileData = new List<TileData>();
            //for (int x = 0; x < 8; x++)
            //{
            //    for (int y = 0; y < 4; y++)
            //    {
            //        if (x > 5 && y == 0) continue; // Skipping bottom right 2 tiles

            //        tileData.Add(new TileData("Stone", x, y));
            //    }
            //}
            //for (int x = 0; x < 8; x++)
            //{
            //    for (int y = 4; y < 8; y++)
            //    {
            //        tileData.Add(new TileData("Grass", x, y));
            //    }
            //}

            //int tilePixelSize = 100;
            //tilemap = new Tilemap(Material.Create(diffuseShader, _mainTexture: tex1/*, _normalTexture: tex2*/, _useLinearFiltering: false, _normalScale: 1),
            //    tileData.ToArray(), tilePixelSize, size, size, tiles);


            ////for (int i = 0; i < LightManager.MAX_POINT_LIGHTS; i++)
            ////{
            ////    Light l = new Light(Color.White, random.Next(1, 8) * 200, random.Next(4, 7), Light.LightType.Point, 2);
            ////    l.GetComponent<Transform>().Position = new Vector2(random.Next(0, size * tilePixelSize), random.Next(0, size * tilePixelSize));
            ////    lights.Add(l);
            ////    lradius.Add(l.Radius);
            ////}
            //#endregion
        }

        Light l = new Light(Color.Blue, 50, 2, Light.LightType.Point, 10);
        protected override void Update()
        {
            l.Transform.Position = Input.GetMouseWorldPosition();
            //for (int i = 0; i < lights.Count; i++)
            //{
            //    lights[i].Intensity = Easing.EaseInOutSine(slider.Value01);
            //    lights[i].Radius = slider.Value01 * lradius[i];
            //}
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
