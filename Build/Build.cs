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

        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight, "Test Game!")
        {

        }

        protected override void Initialize()
        {  
            Console.WriteLine("Game Started");
        }

        protected override void Start()
        {
            var settings = new FontSystemSettings()
            {
                FontResolutionFactor = 2,
                KernelWidth = 2,
                KernelHeight = 1
            };

            fontSystem = new FontSystem(settings);
            fontSystem.AddFont(File.ReadAllBytes(@"Build/Resources/Fonts/FreeSans/FreeSans.ttf"));

            renderer = new TextRenderer(new Transform(), Material.Create(new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultText.glsl"))));
        }

        protected override void Update()
        {
            CameraMovement();

            Console.WriteLine(PerformanceTimings.FramesPerSecond);
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
            var text = "The quick brown fox jumps over the lazy dog.";
            var scale = new Vector2(2, 2);

            var font = fontSystem.GetFont(32);

            var size = font.MeasureString(text, scale);
            var origin = new Vector2(size.X / 2.0f, size.Y / 2.0f);


            //font.DrawText()
        }
    }
}
