using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering.Shaders;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    /// <summary>
    /// Handles rendering textures to the screen using vertices and a shader
    /// </summary>
    public class SpriteRenderer
    {
        private readonly float[] vertices =
        {
            // Positions          UV            Color               Index
             0.5f,  0.5f, 0.0f,   1.0f, 1.0f,   1.0f, 1.0f, 1.0f,   1.0f,      // top right - red
             0.5f, -0.5f, 0.0f,   1.0f, 0.0f,   1.0f, 1.0f, 1.0f,   1.0f,      // bottom right - green
            -0.5f, -0.5f, 0.0f,   0.0f, 0.0f,   1.0f, 1.0f, 1.0f,   1.0f,      // bottom left - blue
            -0.5f,  0.5f, 0.0f,   0.0f, 1.0f,   1.0f, 1.0f, 1.0f,   1.0f,      // top left - white
        };

        private readonly uint[] indices =
        {
            0, 1, 3, // Triangle 1
            1, 2, 3  // Triangle 2
        };

        private VertexBuffer vertexBuffer;
        private VertexArray vertexArray;
        private IndexBuffer indexBuffer;

        private Transform transform;
        private Shader shader;

        public bool loaded { get; private set; } = false;

        public SpriteRenderer(Transform _transform, Shader _shader)
        {
            transform = _transform;
            shader = _shader;
        }

        public void SetShader(Shader _shader)
        {
            shader = _shader;
        }

        public unsafe void Load()
        {
            shader = new Shader(Shader.ParseShader("Build/Resources/Shaders/Default.glsl"));
            if (!shader.CompileShader())
            {
                Console.WriteLine("Failed to compile shader.");
                return;
            }

            vertexArray = new VertexArray();
            vertexBuffer = new VertexBuffer(vertices);

            BufferLayout layout = new BufferLayout();
            layout.Add<float>(3);
            layout.Add<float>(2);
            layout.Add<float>(3);
            layout.Add<float>(1);

            vertexArray.AddBuffer(vertexBuffer, layout);
            shader.Use();
            indexBuffer = new IndexBuffer(indices);

            var textureSampleUniformLocation = shader.GetUniformLocation("u_Texture[0]");
            int[] samplers = new int[2] { 0, 1 };
            glUniform1iv(textureSampleUniformLocation, samplers.Length, samplers);

            loaded = true;
        }

        public unsafe void Render()
        {
            if (!loaded || shader.compiled == false) return;

            shader.SetMatrix4x4("model", transform.GetScaleMatrix() * transform.GetRotationMatrix() * transform.GetPositionMatrix()); // MUST BE IN ORDER

            shader.Use();
            vertexArray.Bind();
            indexBuffer.Bind();

            shader.SetMatrix4x4("projection", Camera2D.main.GetProjectionMatrix()); // MUST be set after Use is called

            glDrawElements(GL_TRIANGLES, indices.Length, GL_UNSIGNED_INT, (void*)0);
        }
    }
}
