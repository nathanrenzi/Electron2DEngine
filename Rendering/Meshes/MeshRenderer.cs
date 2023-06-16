using Electron2D.Framework;
using Electron2D.GameObjects;
using Electron2D.Rendering.Cameras;
using Electron2D.Rendering.Display;
using Electron2D.Rendering.Images;
using Electron2D.Rendering.Shaders;
using StbImageSharp;
using System.Numerics;
using System.Reflection;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Rendering.Meshes
{
    /// <summary>
    /// Handles rendering objects to the screen using vertices and a shader
    /// </summary>
    public class MeshRenderer
    {
        public float[] vertices { get; private set; }
        public uint vao { get; private set; }
        public uint vbo { get; private set; }

        public bool initialized { get; private set; } = false;
        public bool loaded { get; private set; } = false;

        private ImageTexture image;
        private Shader shader;
        private Transform transform;

        public MeshRenderer() { }

        public MeshRenderer(Transform _transform, Shader _shader)
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
            vertices = _transform.vertices;
            initialized = true;
        }

        public void SetShader(Shader _shader)
        {
            shader = _shader;
            shader.Load();
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
            shader.Load();

            // Create our VAO and VBO
            vao = glGenVertexArray();
            vbo = glGenBuffer();

            glBindVertexArray(vao);
            glBindBuffer(GL_ARRAY_BUFFER, vbo);

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

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);

            uint texture;
            glGenTextures(1, &texture);

            // Binding texture to uint - defines how many textures will be used for this object
            glBindTexture(GL_TEXTURE_2D, texture);

            // Setting texture wrapping / filtering options on bound texture object
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

            // Enabling texture alpha channel blending
            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            image.Use(0);

            loaded = true;
        }

        public void Render()
        {
            if (!loaded) return;

            shader.SetMatrix4x4("model", transform.GetScaleMatrix() * transform.GetRotationMatrix() * transform.GetPositionMatrix()); // MUST BE IN ORDER

            shader.Use();
            shader.SetMatrix4x4("projection", Camera2D.main.GetProjectionMatrix()); // MUST be set after Use is called

            glBindVertexArray(vao);
            glDrawArrays(GL_TRIANGLES, 0, 6);
        }
    }
}
