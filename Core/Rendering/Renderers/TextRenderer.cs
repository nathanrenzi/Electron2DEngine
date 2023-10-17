using Electron2D.Core.ECS;
using Electron2D.Core.Management;
using FontStashSharp.Interfaces;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    //https://github.com/FontStashSharp/FontStashSharp/tree/main/samples/FontStashSharp.Samples.OpenTK
    public class TextRenderer : Component, IFontStashRenderer2
    {
        private const int MAX_SPRITES = 128;
        private const int MAX_VERTICES = MAX_SPRITES * 8 * 4;
        private const int MAX_INDICES = MAX_SPRITES * 6;

        private readonly float[] vertices;
        private static readonly uint[] indices = GenerateIndexArray();

        public VertexBuffer vertexBuffer;
        public VertexArray vertexArray;
        public IndexBuffer indexBuffer;
        public BufferLayout layout;
        public Material Material;

        private int vertexIndex = 0;
        private Transform transform;
        private object lastTexture;

        public TextRenderer(Transform _transform, Material _material)
        {
            transform = _transform;
            Material = _material;

            vertices = new float[MAX_VERTICES];

            vertexBuffer = new VertexBuffer(vertices);
            indexBuffer = new IndexBuffer(indices);

            layout = new BufferLayout();
            layout.Add<float>(2);
            layout.Add<float>(2);
            layout.Add<float>(4);

            vertexArray = new VertexArray();
            vertexArray.AddBuffer(vertexBuffer, layout);
        }

        public ITexture2DManager TextureManager => ResourceManager.Instance;

        public void Begin()
        {
            Material.Use();
            Material.Shader.SetMatrix4x4("model", transform.GetScaleMatrix() * transform.GetRotationMatrix() * transform.GetPositionMatrix());
            Material.Shader.SetMatrix4x4("projection", Camera2D.main.GetUnscaledProjectionMatrix());
            vertexArray.Bind();
            indexBuffer.Bind();
            vertexBuffer.Bind();
        }

        public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
        {
            if(lastTexture != texture)
            {
                FlushBuffer();
            }

            SetVertexData(ref topLeft);
            SetVertexData(ref topRight);
            SetVertexData(ref bottomLeft);
            SetVertexData(ref bottomRight);

            lastTexture = texture;
        }

        private void SetVertexData(ref VertexPositionColorTexture _data)
        {
            vertices[vertexIndex++] = _data.Position.X;
            vertices[vertexIndex++] = -_data.Position.Y; // Vertically flipping the mesh
            vertices[vertexIndex++] = _data.TextureCoordinate.X;
            vertices[vertexIndex++] = _data.TextureCoordinate.Y;
            vertices[vertexIndex++] = _data.Color.R;
            vertices[vertexIndex++] = _data.Color.G;
            vertices[vertexIndex++] = _data.Color.B;
            vertices[vertexIndex++] = _data.Color.A;
        }

        public void End()
        {
            FlushBuffer();
        }

        private unsafe void FlushBuffer()
        {
            if(vertexIndex == 0 || lastTexture == null)
            {
                return;
            }

            vertexBuffer.UpdateData(vertices);

            // Binding the texture
            uint texture = (uint)lastTexture;
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, texture);

            // There are 8 floats * 4 vertices per quad, and 6 indices per quad
            glDrawElements(GL_TRIANGLES, vertexIndex / 32 * 6, GL_UNSIGNED_INT, (void*)0);
            vertexIndex = 0;
        }

        protected override void OnDispose()
        {
            vertexBuffer.Dispose();
            vertexArray.Dispose();
            indexBuffer.Dispose();
        }

        private static uint[] GenerateIndexArray()
        {
            uint[] result = new uint[MAX_INDICES];
            for (int i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4)
            {
                result[i] = (uint)(j);
                result[i + 1] = (uint)(j + 1);
                result[i + 2] = (uint)(j + 2);
                result[i + 3] = (uint)(j + 3);
                result[i + 4] = (uint)(j + 2);
                result[i + 5] = (uint)(j + 1);
            }
            return result;
        }
    }
}
