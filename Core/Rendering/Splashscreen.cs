using Electron2D.Core.Audio;
using Electron2D.Core.Management;
using Electron2D.Core.Rendering.Shaders;
using GLFW;
using System.Drawing;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public static class Splashscreen
    {
        private static readonly float[] vertices =
        {
             1f,  1f,       1.0f, 1.0f,
             1f, -1f,       1.0f, 0.0f,
            -1f, -1f,       0.0f, 0.0f,
            -1f,  1f,       0.0f, 1.0f,
        };

        private static readonly uint[] indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private static Texture2D splashTexture;
        private static VertexBuffer vertexBuffer;
        private static VertexArray vertexArray;
        private static IndexBuffer indexBuffer;
        private static BufferLayout layout;
        private static AudioInstance audio;
        private static bool hasPlayedAudio = false;

        public static void Initialize(string _textureName, string _audioGUID, bool _loadAsNonSRGBA)
        {
            splashTexture = TextureFactory.Load(_textureName, _loadAsNonSRGBA);
            vertexArray = new VertexArray();
            vertexBuffer = new VertexBuffer(vertices);
            indexBuffer = new IndexBuffer(indices);
            layout = new BufferLayout();
            layout.Add<float>(2);
            layout.Add<float>(2);
            vertexArray.AddBuffer(vertexBuffer, layout);

            audio = AudioSystem.CreateInstance(_audioGUID);
        }

        public static unsafe void RenderSplashscreen(int _alpha, bool _audioReady)
        {
            if (!hasPlayedAudio && _audioReady)
            {
                audio.Play();
                hasPlayedAudio = true;
            }

            Shader shader = GlobalShaders.DefaultInterface;
            shader.Use();
            shader.SetColor("mainColor", Color.FromArgb(_alpha, Color.White));
            shader.SetMatrix4x4("model", Matrix4x4.Identity);
            shader.SetMatrix4x4("projection", Matrix4x4.Identity);

            splashTexture.Use(GL_TEXTURE0);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            vertexArray.Bind();
            indexBuffer.Bind();
            glDrawElements(GL_TRIANGLES, indices.Length, GL_UNSIGNED_INT, (void*)0);
        }

        public static void Dispose()
        {
            vertexBuffer.Dispose();
            vertexArray.Dispose();
            indexBuffer.Dispose();
        }
    }
}
