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
            SetBackgroundColor(Color.LightBlue);

            var settings = new FontSystemSettings()
            {
                FontResolutionFactor = 2,
                KernelWidth = 2,
                KernelHeight = 2
            };

            fontSystem = new FontSystem(settings);
            fontSystem.AddFont(File.ReadAllBytes("Build/Resources/Fonts/FreeSans/FreeSans.ttf"));

            renderer = new TextRenderer(new Transform(), Material.Create(new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultText.glsl"))));
            Material m = Material.Create(new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultTexture.glsl")));
            sprite = new Sprite(m);
            sprite.Transform.Position = new Vector2(0, 300);
            sprite.Transform.Scale = new Vector2(400, 400);
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
            var text = "A";
            var scale = new Vector2(4, 4);

            var font = fontSystem.GetFont(32);

            var size = font.MeasureString(text, scale);
            var origin = new Vector2(size.X / scale.X, size.Y / scale.Y);

            renderer.Begin();
            font.DrawText(renderer, text, new Vector2(0, 0), FSColor.White, scale, origin: origin);
            renderer.End();
            sprite.Renderer.Material.MainTexture = ResourceManager.Instance.GetTexture(3);
        }
    }
}
