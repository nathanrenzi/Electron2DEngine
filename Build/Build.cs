using GLFW;
using Electron2D.Core;
using System.Numerics;
using Electron2D.Core.Rendering;
using System.Drawing;
using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;
using Electron2D.Core.Rendering.Text;
using Electron2D.Core.Management;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering.Renderers;

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

        TestTextRenderer renderer;
        FontGlyphStore fgh;
        Shader shader;
        private void InitializeFreeType()
        {
            fgh = ResourceManager.Instance.LoadFont("Build/Resources/Fonts/OpenSans.ttf", 100);
            shader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultText.glsl"));
            shader.Compile();
            renderer = new TestTextRenderer();
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
            shader.Use();
            shader.SetMatrix4x4("projection", Camera2D.main.GetUnscaledProjectionMatrix());
            renderer.Render(fgh, shader, "Electron2D", -(1920/2) + 20, -(1080/2) + 20, 1, Color.Red);
        }
    }
}
