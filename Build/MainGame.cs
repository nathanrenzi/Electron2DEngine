using static Electron2D.OpenGL.GL;
using GLFW;
using Electron2D.Core;
using Electron2D.Core.Management;
using Electron2D.Build.Resources.Objects;
using Electron2D.Core.GameObjects;
using System.Numerics;

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
            //BoidField b = new(3, 100);
            GameObject obj = new();

            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/boid1.png");
            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/boid2.png");
            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/mouseBorder.png");
        }

        protected override void Update()
        {
            startCamera.position = Vector2.UnitY * ((float)Math.Sin(Time.totalElapsedSeconds) * 100);
            Console.WriteLine(startCamera.position);
        }

        protected unsafe override void Render()
        {

        }
    }
}
