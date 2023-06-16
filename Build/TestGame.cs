using static Electron2D.OpenGL.GL;
using GLFW;
using Electron2D.Core.Rendering.Display;
using Electron2D.Core.Rendering.Cameras;
using Electron2D.Core.GameObjects;
using Electron2D.Core;
using System.Numerics;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering;
using Electron2D.Core.Management;

namespace Electron2D.Build
{
    // START HERE https://www.youtube.com/watch?v=o9rVMugd2oQ&list=PL65gBgyEEQ9l04ueCI_DLgZW-4hbYHi0H&index=6

    public class TestGame : Game
    {
        private Camera2D camera;

        private readonly float[] vertices =
        {
            //Positions         //Colors
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };

        private uint[] indices =
{
            0, 1, 3, // Triangle 1
            1, 2, 3  // Triangle 2
        };

        private uint vertexArrayObject;
        private uint vertexBufferObject;
        private uint elementBufferObject;

        private Shader shader;
        private Texture2D texture;


        public TestGame(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle) : base(_initialWindowWidth, _initialWindowHeight, _initialWindowTitle)
        {

        }

        protected override void Initialize()
        {
            Console.WriteLine("Game Started");
        }

        protected unsafe override void LoadContent()
        {
            camera = new Camera2D(DisplayManager.windowSize / 2, 1);

            shader = new Shader(Shader.ParseShader("Build/Resources/Shaders/Texture.glsl"));
            shader.CompileShader();
            vertexBufferObject = glGenBuffer();
            glBindBuffer(GL_ARRAY_BUFFER, vertexBufferObject);
            fixed (float* v = &vertices[0])
            {
                glBufferData(GL_ARRAY_BUFFER, vertices.Length * sizeof(float), v, GL_STATIC_DRAW);
            }

            vertexArrayObject = glGenVertexArray();
            glBindVertexArray(vertexArrayObject);

            glVertexAttribPointer(0, 3, GL_FLOAT, false, 5 * sizeof(float), (void*)0);
            glEnableVertexAttribArray(0);

            glVertexAttribPointer(1, 2, GL_FLOAT, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));
            glEnableVertexAttribArray(1);

            elementBufferObject = glGenBuffer();
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, elementBufferObject);
            fixed (uint* i = &indices[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, indices.Length * sizeof(uint), i, GL_STATIC_DRAW);

            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            texture = ResourceManager.Instance.LoadTexture("Build/Resources/Textures/boid1.png");
            texture.Use();

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

            shader.Use();
            glBindVertexArray(vertexArrayObject);
            glDrawElements(GL_TRIANGLES, indices.Length, GL_UNSIGNED_INT, (void*)0);
            GameObjectManager.RenderGameObjects();

            Glfw.SwapBuffers(DisplayManager.window);
        }
    }
}
