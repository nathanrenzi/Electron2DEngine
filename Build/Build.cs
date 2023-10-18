using GLFW;
using Electron2D.Core;
using Electron2D.Core.Management;
using System.Numerics;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.ECS;
using System.Drawing;
using Electron2D.Core.Misc;
using Electron2D.Core.Management.Textures;
using FontStashSharp;

namespace Electron2D.Build
{
    public class Build : Game
    {
        private List<Entity> lightObj = new List<Entity>();
        private Tilemap tilemap;
        private FontSystem fontSystem;
        private TextRenderer renderer;
        private Sprite sprite;

        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight, "Test Game!")
        {

        }

        protected override void Initialize()
        {  
            Console.WriteLine("Game Started");
        }

        protected override void Start()
        {
            Texture2D tex1 = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/tiles1.png");
            Texture2D tex2 = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/tilesNormal1.png", true);
            SpritesheetManager.Add(tex1, 2, 2);

            Shader diffuseShader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultLit.glsl"),
                _globalUniformTags: new string[] { "lights" });

            int size = 100;
            int[] tiles = new int[size * size];
            Random random = new Random();
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = random.Next(0, 2) == 0 ? 0 : (random.Next(0, 6) > 0 ? 1 : 2);
            }

            int tilePixelSize = 100;
            tilemap = new Tilemap(Material.Create(diffuseShader, _mainTexture: tex1, _normalTexture: tex2, _useLinearFiltering: false, _normalScale: 1),
                new TileData[] { new TileData("Grass1", 0, 1), new TileData("Grass2", 1, 1), new TileData("Pebble", 0, 0) }, tilePixelSize, size, size, tiles);

            tilemap.GetComponent<Transform>().Position = new Vector2(-540, -540);


            int numLights = 64;
            for (int i = 0; i < numLights; i++)
            {
                Entity light = new Entity();
                light.AddComponent(new Transform());
                light.AddComponent(new Light(Color.White, random.Next(1, 8) * 100, random.Next(1, 3), Light.LightType.Point, 2));
                light.GetComponent<Transform>().Position = new Vector2(random.Next(0, size * tilePixelSize), random.Next(0, size * tilePixelSize));

                lightObj.Add(light);
            }

            //SetBackgroundColor(Color.LightBlue);

            //var settings = new FontSystemSettings()
            //{
            //    FontResolutionFactor = 2,
            //    KernelWidth = 2,
            //    KernelHeight = 2
            //};

            //fontSystem = new FontSystem(settings);
            //fontSystem.AddFont(File.ReadAllBytes("Build/Resources/Fonts/FreeSans/FreeSans.ttf"));

            //renderer = new TextRenderer(new Transform(), Material.Create(new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultText.glsl"))));
            //Material m = Material.Create(new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultTexture.glsl")));
            //sprite = new Sprite(m);
            //sprite.Transform.Position = new Vector2(0, 300);
            //sprite.Transform.Scale = new Vector2(400, 400);
        }

        protected override void Update()
        {
            CameraMovement();
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

        protected unsafe override void Render()
        {
            //var text = "The quick brown fox jumps over the lazy dog.";
            //var scale = new Vector2(4, 4);

            //var font = fontSystem.GetFont(32);

            //var size = font.MeasureString(text, scale);
            //var origin = new Vector2(size.X / scale.X, size.Y / scale.Y);

            //renderer.Begin();
            //font.DrawText(renderer, text, new Vector2(0, 0), FSColor.White, scale, origin: origin);
            //renderer.End();
            //sprite.Renderer.Material.MainTexture = ResourceManager.Instance.GetTexture(3);
        }
    }
}
