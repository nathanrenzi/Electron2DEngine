using OpenGLTest.GameLoop;
using OpenGLTest.GameObjects;
using OpenGLTest.Rendering.Cameras;
using OpenGLTest.Rendering.Display;
using OpenGLTest.Rendering.Shaders;
using StbImageSharp;
using System.Numerics;
using System.Reflection;
using static OpenGLTest.OpenGL.GL;

namespace OpenGLTest.Rendering.Meshes
{
    /// <summary>
    /// Handles rendering objects to the screen using vertices and a shader
    /// </summary>
    public class MeshRenderer
    {
        public float[] vertices { get; private set; }
        public uint vao { get; private set; }
        public uint vbo { get; private set; }

        private string imagesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Images\\");
        private Shader shader;
        private Transform transform;

        private bool initialized = false;

        public MeshRenderer() { }

        public MeshRenderer(Transform _transform, Shader _shader, float[] _vertices)
        {
            Initialize(_transform, _shader, _vertices);
        }

        /// <summary>
        /// Call this method to initialize the shader and vertices if they were not passed into the constructor
        /// </summary>
        public void Initialize(Transform _transform, Shader _shader, float[] _vertices)
        {
            transform = _transform;
            shader = _shader;
            vertices = _vertices;
            initialized = true;
        }

        public unsafe void Load()
        {
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

            // Binding texture to uint
            uint texture;
            glGenTextures(1, &texture);
            glBindTexture(GL_TEXTURE_2D, texture);

            // Setting texture wrapping / filtering options on bound texture object
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

            // Enabling texture alpha channel blending
            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

            // Loading and generating texture
            string path = Path.Combine(imagesPath, "test2.png");
            byte[] data = File.ReadAllBytes(path);
            StbImage.stbi_set_flip_vertically_on_load(1);
            ImageResult image = ImageResult.FromMemory(data, ColorComponents.RedGreenBlueAlpha);
            fixed (byte* d = image.Data)
            {
                if (d != null)
                {
                    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, image.Width, image.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, d);
                    glGenerateMipmap(GL_TEXTURE_2D);
                }
                else
                {
                    Console.WriteLine("Failed to load texture.");
                }
            }
        }

        public void Render()
        {
            shader.SetMatrix4x4("model", transform.GetScaleMatrix() * transform.GetRotationMatrix() * transform.GetPositionMatrix()); // MUST BE IN ORDER

            shader.Use();
            shader.SetMatrix4x4("projection", Camera2D.main.GetProjectionMatrix()); // MUST be set after Use is called

            glBindVertexArray(vao);
            glDrawArrays(GL_TRIANGLES, 0, 6);
        }
    }
}
