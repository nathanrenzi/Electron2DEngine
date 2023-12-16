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
        private TextLabel fpsLabel;
        private UiComponent fpsBackground;
        private int displayFrames;
        private int frames;
        private float lastFrameCountTime;
        private Panel mainPanel;
        private SliderSimple slider;

        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight,
            $"Electron2D Build - {Program.BuildDate}") { }

        protected override void Load()
        {
            SetBackgroundColor(Color.LightBlue);

            // Load Custom Component Systems
            // Ex. ComponentSystem.Start();
            // -----------------------------

            InitializeFPSLabel();

            //mainPanel = new Panel(Color.DarkGray, -1, 350, 300);
            //mainPanel.SetLayoutGroup(new ListLayoutGroup(new Vector4(20), 20, ListDirection.Vertical, SizeMode.WidthHeight, SizeMode.None, LayoutAlignment.Left, LayoutAlignment.Top));
            //mainPanel.Layout.AddToLayout(new TextLabel("This is a test of the list layout group.", "Build/Resources/Fonts/OpenSans.ttf",
            //    30, Color.Black, Color.White, new Vector2(0, 0), TextAlignment.Center, TextAlignment.Center,
            //    TextAlignmentMode.Geometry, TextOverflowMode.Word));
            //mainPanel.Layout.AddToLayout(new Panel(Color.Black));
            //mainPanel.Layout.AddToLayout(new Panel(Color.Black));

            slider = new SliderSimple(Color.Black, Color.Green, Color.Khaki, 0.5f, 0, 1, 200, 10, 10, 30);
        }

        protected override void Update()
        {
            CameraMovement();
            CalculateFPS();
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

        private void InitializeFPSLabel()
        {
            fpsLabel = new TextLabel("FPS: 0", "Build/Resources/Fonts/OpenSans.ttf",
                30, Color.White, Color.White, new Vector2(130, 30), TextAlignment.Left, TextAlignment.Center,
                TextAlignmentMode.Geometry, TextOverflowMode.Disabled, _uiRenderLayer: 11);
            fpsBackground = new Panel(Color.Black, 10, 140, 30);

            fpsLabel.Anchor = new Vector2(-1, 1);
            fpsLabel.Transform.Position = new Vector2(-1920 / 2, 1080 / 2);
            fpsBackground.Anchor = new Vector2(-1, 1);
            fpsBackground.Transform.Position = new Vector2(-1920 / 2, 1080 / 2);
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
