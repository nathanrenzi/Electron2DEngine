using static Electron2D.OpenGL.GL;
using GLFW;
using Electron2D.Core;
using Electron2D.Core.Management;
using Electron2D.Build.Resources.Objects;
using Electron2D.Core.GameObjects;
using System.Numerics;
using Electron2D.Core.Rendering;

namespace Electron2D.Build
{
    public class MainGame : Game
    {
        public MainGame(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle) : base(_initialWindowWidth, _initialWindowHeight, _initialWindowTitle)
        {

        }

        protected override void Initialize()
        {  
            Console.WriteLine("Game Started");
        }

        protected override void LoadContent()
        {
            BoidField b = new(3, 100);
            GameObject centerDisplay = new(false);
            centerDisplay.renderer.Load();
            centerDisplay.renderer.SetVertexValueAll(8, 2);
            centerDisplay.renderer.SetVertexValueAll(7, 1);
            centerDisplay.renderer.SetVertexValueAll(4, 1);
            centerDisplay.renderer.SetVertexValueAll(5, 1);
            centerDisplay.renderer.SetVertexValueAll(6, 0.7f);

            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/boid1.png");
            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/boid2.png");
            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/mouseBorder.png");
        }

        protected override void Update()
        {
            Camera2D.main.zoom += Input.scrollDelta;
            Camera2D.main.zoom = Math.Clamp(Camera2D.main.zoom, 1, 3);

            float moveSpeed = 1000;
            if(Input.GetKey(Keys.W))
            {
                Camera2D.main.position += new Vector2(0, moveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(Keys.A))
            {
                Camera2D.main.position += new Vector2(-moveSpeed * Time.deltaTime, 0);
            }
            if (Input.GetKey(Keys.S))
            {
                Camera2D.main.position += new Vector2(0, -moveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(Keys.D))
            {
                Camera2D.main.position += new Vector2(moveSpeed * Time.deltaTime, 0);
            }
        }

        protected unsafe override void Render()
        {

        }
    }
}
