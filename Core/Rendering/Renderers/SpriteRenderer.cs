using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering.Shaders;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    /// <summary>
    /// Handles rendering textures to the screen using vertices and a shader
    /// </summary>
    public class SpriteRenderer : IRenderer
    {
        private readonly float[] vertices =
        {
            // Positions    UV            Color                     Index
             1f,  1f,       1.0f, 1.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // top right - red
             1f, -1f,       1.0f, 0.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // bottom right - green
            -1f, -1f,       0.0f, 0.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // bottom left - blue
            -1f,  1f,       0.0f, 1.0f,   1.0f, 1.0f, 1.0f, 1.0f,   0.0f,      // top left - white
        };

        private readonly uint[] indices =
        {
            0, 1, 3, // Triangle 1
            1, 2, 3  // Triangle 2
        };

        private VertexBuffer vertexBuffer;
        private VertexArray vertexArray;
        private IndexBuffer indexBuffer;
        private BufferLayout layout;

        private Transform transform;
        private Shader shader;

        public bool loaded { get; private set; } = false;

        public SpriteRenderer(Transform _transform, Shader _shader)
        {
            transform = _transform;
            shader = _shader;
        }

        public Shader GetShader() => shader;

        /// <summary>
        /// Loads all resources necessary for the renderer, such as the shader and all buffers.
        /// </summary>
        public void Load()
        {
            if (!shader.CompileShader())
            {
                Console.WriteLine("Failed to compile shader.");
                return;
            }

            vertexArray = new VertexArray();
            vertexBuffer = new VertexBuffer(vertices);

            layout = new BufferLayout();
            layout.Add<float>(2);
            layout.Add<float>(2);
            layout.Add<float>(4);
            layout.Add<float>(1);

            vertexArray.AddBuffer(vertexBuffer, layout);
            shader.Use();
            indexBuffer = new IndexBuffer(indices);

            var textureSampleUniformLocation = shader.GetUniformLocation("u_Texture[0]");
            int[] samplers = new int[3] { 0, 1, 2 };
            glUniform1iv(textureSampleUniformLocation, samplers.Length, samplers);

            loaded = true;
        }

        /// <summary>
        /// Sets a specific value in every vertex.
        /// </summary>
        /// <param name="_type">The type of attribute to edit.</param>
        /// <param name="_value">The value to set.</param>
        public void SetVertexValueAll(int _type, float _value)
        {
            if(!loaded)
            {
                Console.WriteLine("Trying to set vertex data when renderer has not been initialized yet.");
                return;
            }

            int loops = vertices.Length / layout.GetRawStride();

            // Setting the value for each vertex
            for (int i = 0; i < loops; i++)
            {
                vertices[(i * layout.GetRawStride()) + _type] = _value;
            }

            // Setting a new vertex buffer
            vertexBuffer = new VertexBuffer(vertices);
            vertexArray.AddBuffer(vertexBuffer, layout);
        }

        public unsafe void Render()
        {
            if (!loaded || shader.compiled == false) return;

            shader.Use();
            shader.SetMatrix4x4("model", transform.GetScaleMatrix() * transform.GetRotationMatrix() * transform.GetPositionMatrix()); // MUST BE IN ORDER
            vertexArray.Bind();
            indexBuffer.Bind();

            shader.SetMatrix4x4("projection", Camera2D.main.GetProjectionMatrix()); // MUST be set after Use is called

            glDrawElements(GL_TRIANGLES, indices.Length, GL_UNSIGNED_INT, (void*)0);
        }
    }
    
    /// <summary>
    /// This enum corresponds to an attribute in a vertex for the sprite renderer.
    /// </summary>
    public enum SpriteRendererAttribute
    {
        PositionX = 0,
        PositionY = 1,
        UvX = 2,
        UvY = 3,
        ColorR = 4,
        ColorG = 5,
        ColorB = 6,
        ColorA = 7,
        TextureIndex = 8
    }
}
