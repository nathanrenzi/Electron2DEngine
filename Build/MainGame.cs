using static Electron2D.OpenGL.GL;
using GLFW;
using Electron2D.Core.GameObjects;
using Electron2D.Core;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering;
using Electron2D.Core.Management;

namespace Electron2D.Build
{
    // START HERE https://www.youtube.com/watch?v=o9rVMugd2oQ&list=PL65gBgyEEQ9l04ueCI_DLgZW-4hbYHi0H&index=6

    public class MainGame : Game
    {
        private Camera2D camera;

        public MainGame(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle) : base(_initialWindowWidth, _initialWindowHeight, _initialWindowTitle)
        {

        }

        protected override void Initialize()
        {
            Console.WriteLine("Game Started");
        }

        protected override void LoadContent()
        {
            camera = new Camera2D(DisplayManager.windowSize / 2, 1);

            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/boid1.png");
            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/boid2.png");

            // Must be called after all loading is done so that game objects can access loaded content
            GameObjectManager.StartGameObjects();
        }

        protected override void Update()
        {
            GameObjectManager.UpdateGameObjects();
        }

        protected unsafe override void Render()
        {
            glClear(GL_COLOR_BUFFER_BIT);
            glClearColor(0.2f, 0.2f, 0.2f, 1);

            GameObjectManager.RenderGameObjects();

            Glfw.SwapBuffers(DisplayManager.window);
        }
    }
}
