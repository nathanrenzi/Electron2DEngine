using Electron2D.Rendering.Shaders;

using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Rendering
{
    public static class Splashscreen
    {
        private static readonly float[] _vertices =
        {
             1f,  1f,       1.0f, 1.0f,
             1f, -1f,       1.0f, 0.0f,
            -1f, -1f,       0.0f, 0.0f,
            -1f,  1f,       0.0f, 1.0f,
        };

        private static readonly uint[] _indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private static VertexBuffer _vertexBuffer;
        private static VertexArray _vertexArray;
        private static IndexBuffer _indexBuffer;
        private static BufferLayout _layout;
        private static SharedResource<Texture2D> _texture;

        public static void Initialize()
        {
            _vertexArray = new VertexArray();
            _vertexBuffer = new VertexBuffer(_vertices);
            _indexBuffer = new IndexBuffer(_indices);
            _layout = new BufferLayout();
            _layout.Add<float>(2);
            _layout.Add<float>(2);
            _vertexArray.AddBuffer(_vertexBuffer, _layout);
        }

        public static unsafe void Render(SharedResource<Texture2D> texture, float alpha)
        {
            SharedResource<Shader> shader = GlobalShaders.Interface.AddRef();
            shader.Value.Use();
            shader.Value.SetColor("mainColor", Color.White.WithAlpha(alpha));
            shader.Value.SetMatrix4x4("model", Matrix4x4.Identity);
            shader.Value.SetMatrix4x4("uiMatrix", Matrix4x4.Identity);
            shader.Value.SetMatrix4x4("projection", Matrix4x4.Identity);

            _texture = texture.AddRef();
            _texture.Value.Use(GL_TEXTURE0);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            _vertexArray.Bind();
            _indexBuffer.Bind();
            glDrawElements(GL_TRIANGLES, _indices.Length, GL_UNSIGNED_INT, (void*)0);
        }

        public static void Dispose()
        {
            _vertexBuffer.Dispose();
            _vertexArray.Dispose();
            _indexBuffer.Dispose();
            _texture.Release();
        }
    }
}
