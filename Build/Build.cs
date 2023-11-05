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

namespace Electron2D.Build
{
    public class Build : Game
    {
        private Sprite sprite;

        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight, "Test Game!") { }

        protected override void Load()
        {
            SetBackgroundColor(Color.LightBlue);

            InitializeFreeType();
        }

        private void InitializeFreeType()
        {
            FontGlyphStore d = ResourceManager.Instance.LoadFont("Build/Resources/Fonts/FreeSans/FreeSans.ttf", 100);

            sprite = new Sprite(Material.Create(new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultText.glsl"))));
            sprite.Transform.Scale = new Vector2(100, 100);
        }

        int i = 0;
        protected override void Update()
        {
            CameraMovement();
            sprite.Renderer.Material.MainTexture = new Texture2D((uint)i, 100, 100);
            i = i > 100 ? 0 : i + 1;
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

        }
    }
}
