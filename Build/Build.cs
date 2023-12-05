using Electron2D.Core;
using Electron2D.Core.Management;
using Electron2D.Core.Misc;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Renderers;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering.Text;
using GLFW;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Build
{
    public class Build : Game
    {
        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight, "Test Game!") { }

        protected override void Load()
        {
            SetBackgroundColor(Color.LightBlue);

            InitializeFreeType();
        }

        TextRenderer renderer;
        FontGlyphStore fgh;
        Shader shader;
        private void InitializeFreeType()
        {
            fgh = ResourceManager.Instance.LoadFont("Build/Resources/Fonts/NotoSans.ttf", 40, 0);
            shader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultText.glsl"));
            shader.Compile();
            renderer = new TextRenderer(fgh, shader, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean fermentum ante eget tellus ultrices facilisis. Ut sit amet auctor tortor.",
                Vector2.Zero, new Vector2(500, 500), Color.Black, Color.Black,
                TextAlignment.Left, TextAlignment.Top, TextAlignmentMode.Baseline);
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
            renderer.Render();
            //renderer.Position.X = MathF.Sin(Time.TotalElapsedSeconds / 2f) * 200;
        }
    }
}
