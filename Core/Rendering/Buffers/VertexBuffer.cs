using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class VertexBuffer : IBuffer
    {
        public uint BufferID { get; }

        public unsafe VertexBuffer(float[] _vertices)
        {
            BufferID = glGenBuffer();
            glBindBuffer(GL_ARRAY_BUFFER, BufferID);
            fixed (float* v = &_vertices[0])
                glBufferData(GL_ARRAY_BUFFER, _vertices.Length * sizeof(float), v, GL_STATIC_DRAW);
        }

        public unsafe void UpdateData(float[] _vertices)
        {
            glBindBuffer(GL_ARRAY_BUFFER, BufferID);
            fixed (float* v = &_vertices[0])
                glBufferData(GL_ARRAY_BUFFER, _vertices.Length * sizeof(float), v, GL_STATIC_DRAW);
        }

        public void Bind()
        {
            glBindBuffer(GL_ARRAY_BUFFER, BufferID);
        }

        public void Unbind()
        {
            glBindBuffer(GL_ARRAY_BUFFER, 0);
        }

        public void Dispose()
        {
            glDeleteBuffer(BufferID);
        }
    }
}
