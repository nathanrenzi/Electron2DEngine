using GLFW;
using Electron2D.Core;
using System.Numerics;
using Electron2D.Core.Rendering;
using System.Drawing;
using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;
using Electron2D.Core.Rendering.Text;
using Electron2D.Core.Management;

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

        private void InitializeFreeType()
        {
            FontGlyphStore d = ResourceManager.Instance.LoadFont("Build/Resources/Fonts/FreeSans/FreeSans.ttf", 30);
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

        }
    }
}
