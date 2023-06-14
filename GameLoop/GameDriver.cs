using static OpenGLTest.OpenGL.GL;
using GLFW;
using StbImageSharp;
using OpenGLTest.Rendering.Display;
using OpenGLTest.Rendering.Shaders;
using OpenGLTest.Rendering.Cameras;
using OpenGLTest.Rendering.Meshes;
using OpenGLTest.GameObjects;

namespace OpenGLTest.GameLoop
{
    public class GameDriver : BaseGame
    {
        private GameObject gameObject;
        private Camera2D cam;

        private bool loaded = false;

        public GameDriver(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle) : base(_initialWindowWidth, _initialWindowHeight, _initialWindowTitle)
        {

        }

        protected override void Initialize()
        {
            Console.WriteLine("Game Started");
        }

        protected unsafe override void LoadContent()
        {
            ShaderLoader.Initialize();

            Shader shader = ShaderLoader.GetShader(DefaultShaderType.SPRITE);
            gameObject = new GameObject();
            gameObject.InitializeMeshRenderer(shader);
            gameObject.transform.position = new System.Numerics.Vector2(initialWindowWidth / 2f, initialWindowHeight / 2f);
            gameObject.transform.scale = new System.Numerics.Vector2(initialWindowHeight / 4f, initialWindowHeight / 4f);

            cam = new Camera2D(DisplayManager.windowSize / 2, 2.5f);

            loaded = true;

            // Must be called after all loading is done so that game objects can access loaded content
            GameObjectManager.StartGameObjects();
        }
         
        protected override void Update()
        {
            if (!loaded) return;

            gameObject.transform.position = new System.Numerics.Vector2(initialWindowWidth / 2f + ((float)Math.Sin(Time.totalElapsedSeconds) * initialWindowWidth / 8f),
                initialWindowHeight / 2f + ((float)Math.Cos(Time.totalElapsedSeconds) * initialWindowHeight / 8f));
            GameObjectManager.UpdateGameObjects();
        }

        protected override void Render()
        {
            if (!loaded) return;

            glClearColor(0.5f, 0, 0, 0);
            glClear(GL_COLOR_BUFFER_BIT);

            GameObjectManager.RenderGameObjects();

            Glfw.SwapBuffers(DisplayManager.window);
        }
    }
}
