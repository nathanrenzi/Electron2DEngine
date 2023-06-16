using static Electron2D.OpenGL.GL;
using GLFW;
using StbImageSharp;
using Electron2D.Rendering.Display;
using Electron2D.Rendering.Shaders;
using Electron2D.Rendering.Cameras;
using Electron2D.Rendering.Meshes;
using Electron2D.GameObjects;
using System.Numerics;
using Electron2D.Framework;
using OpenGLTest.Game.Objects.Boids;

namespace OpenGLTest.Game
{
    public class MainDriver : DriverClass
    {
        private Camera2D camera;

        private bool loaded = false;

        public MainDriver(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle) : base(_initialWindowWidth, _initialWindowHeight, _initialWindowTitle)
        {

        }

        protected override void Initialize()
        {
            Console.WriteLine("Game Started");
        }

        protected unsafe override void LoadContent()
        {
            ShaderLoader.Initialize();

            BoidField boidField = new BoidField();

            camera = new Camera2D(DisplayManager.windowSize / 2, 1);

            loaded = true;

            // Must be called after all loading is done so that game objects can access loaded content
            GameObjectManager.StartGameObjects();
        }

        protected override void Update()
        {
            if (!loaded) return;

            GameObjectManager.UpdateGameObjects();
        }

        protected override void Render()
        {
            if (!loaded) return;

            glClearColor(0.2f, 0.2f, 0.2f, 1);
            glClear(GL_COLOR_BUFFER_BIT);

            GameObjectManager.RenderGameObjects();

            Glfw.SwapBuffers(DisplayManager.window);
        }
    }
}
