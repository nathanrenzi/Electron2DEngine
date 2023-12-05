using Electron2D.Core;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Text;
using Electron2D.Core.UI;
using Electron2D.Core.UserInterface;
using GLFW;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Build
{
    public class Build : Game
    {
        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight, "Test Game!") { }

        TextLabel label;
        UiComponent bg;
        protected override void Load()
        {
            SetBackgroundColor(Color.LightBlue);

            // Load Custom Component Systems
            // -----------------------------

            bg = new UiComponent(-1, 100, 100);
            bg.SetColor(Color.DarkGray);

            label = new TextLabel("FPS: 165", "Build/Resources/Fonts/NotoSans.ttf",
                30, Color.Black, Color.White, new Vector2(150, 30), TextAlignment.Left, TextAlignment.Center,
                TextAlignmentMode.Geometry, TextOverflowMode.Disabled);
            label.GetComponent<TextRenderer>().ShowBoundsDebug = true;
            UiConstraint constraint = new PixelConstraint(20, UiConstraintSide.Left);
            UiConstraint constraint2 = new PixelConstraint(20, UiConstraintSide.Top);

            label.Constraints.SetPosition(constraint);
            label.Constraints.SetPosition(constraint2);
            Debug.Log(label.Transform.Position);
            //bg.Constraints.SetPosition(constraint);
            //bg.Constraints.SetPosition(constraint2);
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
