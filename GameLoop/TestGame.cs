using static OpenGLTest.OpenGL.GL;
using GLFW;
using StbImageSharp;
using OpenGLTest.Rendering.Display;
using OpenGLTest.Rendering.Shaders;
using OpenGLTest.Rendering.Cameras;
using System.Numerics;
using System.Reflection;

namespace OpenGLTest.GameLoop
{
    public class TestGame : Game
    {
        private uint vao;
        private uint vbo;

        private Shader shader;
        private Camera2D cam;

        public TestGame(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle) : base(_initialWindowWidth, _initialWindowHeight, _initialWindowTitle)
        {

        }

        protected override void Initialize()
        {
            
        }

        protected unsafe override void LoadContent()
        {
            string vertexShader = @"#version 330 core
                                    layout (location = 0) in vec2 aPosition;
                                    layout (location = 1) in vec2 aTexCoord;
                                    layout (location = 2) in vec3 aColor;
                                    out vec4 vertexColor;
                                    out vec2 texCoord;
    
                                    uniform mat4 projection;
                                    uniform mat4 model;

                                    void main() 
                                    {
                                        vertexColor = vec4(aColor.rgb, 1.0);
                                        gl_Position = projection * model * vec4(aPosition.xy, 0, 1.0);
                                        texCoord = aTexCoord;
                                    }";

            string fragmentShader = @"#version 330 core
                                    out vec4 FragColor;

                                    in vec4 vertexColor;
                                    in vec2 texCoord;

                                    uniform sampler2D mainTexture;

                                    void main() 
                                    {
                                        FragColor = texture(mainTexture, texCoord);
                                    }";

            shader = new Shader(vertexShader, fragmentShader);
            shader.Load();

            // Create our VAO and VBO
            vao = glGenVertexArray();
            vbo = glGenBuffer();

            glBindVertexArray(vao);
            glBindBuffer(GL_ARRAY_BUFFER, vbo);

            float[] vertices =
            {
                // Position    UV        Color
                -0.5f, 0.5f,   0f, 0f,   1f, 0f, 0f,    // top left
                0.5f, 0.5f,    1f, 0f,   0f, 1f, 0f,    // top right
                -0.5f, -0.5f,  0f, 1f,   0f, 0f, 1f,    // bottom left

                0.5f, 0.5f,    1f, 0f,   0f, 1f, 0f,    // top right
                0.5f, -0.5f,   1f, 1f,   0f, 1f, 1f,    // bottom right
                -0.5f, -0.5f,  0f, 1f,   0f, 0f, 1f,    // bottom left
            };

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
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

            // Loading and generating texture
            string imagesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Images\\");
            string path = Path.Combine(imagesPath, "test.png");
            byte[] data = File.ReadAllBytes(path);
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

            cam = new Camera2D(DisplayManager.windowSize / 2, 2.5f);
        }
         
        protected override void Update()
        {

        }

        protected override void Render()
        {
            glClearColor(0, 0, 0, 0);
            glClear(GL_COLOR_BUFFER_BIT);

            Vector2 position = new Vector2(400, 300);
            Vector2 scale = new Vector2(150, 100);
            float rotation = MathF.Sin(Time.totalElapsedSeconds) * MathF.PI;

            Matrix4x4 trans = Matrix4x4.CreateTranslation(position.X, position.Y, 0);
            Matrix4x4 sca = Matrix4x4.CreateScale(scale.X, scale.Y, 1);
            Matrix4x4 rot = Matrix4x4.CreateRotationZ(rotation);

            shader.SetMatrix4x4("model", sca * rot * trans); // MUST BE IN ORDER

            shader.Use();
            shader.SetMatrix4x4("projection", cam.GetProjectionMatrix()); // MUST be set after Use is called

            glBindVertexArray(vao);
            glDrawArrays(GL_TRIANGLES, 0, 6);

            Glfw.SwapBuffers(DisplayManager.window);
        }
    }
}
