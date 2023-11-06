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
            renderer = new TextRenderer(fgh, shader, new Vector2(-(1920 / 2) + 3, (1080 / 2) - fgh.Arguments.FontSize), 1, Color.Maroon, Color.Black);
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

        float fpsTime = 0;
        int frames = 0;
        int fps = 0;
        protected unsafe override void Render()
        {
            fpsTime += Time.DeltaTime;
            if (fpsTime >= 1)
            {
                fpsTime -= 1;
                fps = frames;
                frames = 0;
            }
            frames++;

            shader.Use();
            shader.SetMatrix4x4("projection", Camera2D.main.GetUnscaledProjectionMatrix());
            renderer.Render($"FPS: {fps}");
        }
    }
}
