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
    public class TestGame : Game
    {
        private GameObject gameObject;
        private Camera2D cam;

        private bool loaded = false;

        public TestGame(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle) : base(_initialWindowWidth, _initialWindowHeight, _initialWindowTitle)
        {

        }

        protected override void Initialize()
        {
            Console.WriteLine("Game Started");
        }

        protected unsafe override void LoadContent()
        {
            string vertex = ShaderLoader.GetShader("SpriteShader", ShaderType.SHADER_VERTEX);
            string fragment = ShaderLoader.GetShader("SpriteShader", ShaderType.SHADER_FRAGMENT);

            if (vertex == "" || fragment == "") return;

            Shader shader = new Shader(vertex, fragment);

            gameObject = new GameObject();
            gameObject.InitializeMeshRenderer(shader);

            cam = new Camera2D(DisplayManager.windowSize / 2, 2.5f);

            loaded = true;
        }
         
        protected override void Update()
        {

        }

        protected override void Render()
        {
            if (!loaded) return;

            glClearColor(0.5f, 0, 0, 0);
            glClear(GL_COLOR_BUFFER_BIT);

            gameObject.Render();

            Glfw.SwapBuffers(DisplayManager.window);
        }
    }
}
