using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering.Cameras;
using Electron2D.Core.Rendering.Images;
using Electron2D.Core.Rendering.Shaders;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering.Meshes
{
    /// <summary>
    /// Handles rendering textures to the screen using vertices and a shader
    /// </summary>
    public class TextureRenderer
    {
        public readonly float[] vertices =
        {
                // Position    UV        Color
                1f, 1f,        1f, 0f,   0f, 1f, 0f,    // top right
                1f, -1f,       1f, 1f,   0f, 1f, 1f,    // bottom right
                -1f, -1f,      0f, 1f,   0f, 0f, 1f     // bottom left
                -1f, 1f,       0f, 0f,   1f, 0f, 0f,    // top left
        };

        private uint[] indices =
        {
            0, 1, 3, // Triangle 1
            1, 2, 3  // Triangle 2
        };

        private uint vertexArrayObject;
        private uint vertexBufferObject;
        private uint elementBufferObject;

        public bool loaded { get; private set; } = false;

        private ImageTexture image;
        private Shader shader;
        private Transform transform;

        public TextureRenderer() { }

        public TextureRenderer(Transform _transform, Shader _shader)
        {
            Initialize(_transform, _shader);
        }

        /// <summary>
        /// Call this method to initialize the shader and vertices if they were not passed into the constructor
        /// </summary>
        public void Initialize(Transform _transform, Shader _shader)
        {
            transform = _transform;
            shader = _shader;
        }

        public void SetShader(Shader _shader)
        {
            shader = _shader;
        }

        public void SetImage(ImageTexture _image)
        {
            image = _image;
        }

        public unsafe void Load()
        {
            if (shader == null)
            {
                Console.WriteLine("No shader found on GameObject. Loading aborted.");
                return;
            }

            // Create our VAO and VBO
            vertexArrayObject = glGenVertexArray();
            vertexBufferObject = glGenBuffer();

            glBindVertexArray(vertexArrayObject);
            glBindBuffer(GL_ARRAY_BUFFER, vertexBufferObject);

            fixed (float* v = &vertices[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_STATIC_DRAW);
            }

            // Vertices
            glVertexAttribPointer(0, 2, GL_FLOAT, false, 7 * sizeof(float), (void*)0);
            glEnableVertexAttribArray(0);

            // UV
            glVertexAttribPointer(1, 2, GL_FLOAT, false, 7 * sizeof(float), (void*)(2 * sizeof(float)));
            glEnableVertexAttribArray(1);

            // Colors
            glVertexAttribPointer(2, 3, GL_FLOAT, false, 7 * sizeof(float), (void*)(4 * sizeof(float)));
            glEnableVertexAttribArray(2);

            glBindVertexArray(0);
            glBindBuffer(GL_ARRAY_BUFFER, 0);

            //elementBufferObject = glGenBuffer();
            //glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, elementBufferObject);
            //fixed (uint* i = &indices[0])
            //{
            //    glBufferData(GL_ELEMENT_ARRAY_BUFFER, indices.Length * sizeof(uint), i, GL_STATIC_DRAW);
            //}

            //uint texture;
            //glGenTextures(1, &texture);

            //// Binding texture to uint - defines how many textures will be used for this object
            //glBindTexture(GL_TEXTURE_2D, texture);

            //// Setting texture wrapping / filtering options on bound texture object
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

            //// Enabling texture alpha channel blending
            //glEnable(GL_BLEND);
            //glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            //image.Use(0);

            loaded = true;
        }

        public unsafe void Render()
        {
            if (!loaded || shader.compiled == false) return;

            shader.SetMatrix4x4("model", transform.GetScaleMatrix() * transform.GetRotationMatrix() * transform.GetPositionMatrix()); // MUST BE IN ORDER

            shader.Use();
            shader.SetMatrix4x4("projection", Camera2D.main.GetProjectionMatrix()); // MUST be set after Use is called

            glBindVertexArray(vertexArrayObject);
            //fixed (uint* i = &indices[0])
            //    glDrawElements(GL_TRIANGLES, indices.Length, GL_UNSIGNED_INT, i);
            glDrawArrays(GL_TRIANGLES, 0, 4);
        }
    }
}
