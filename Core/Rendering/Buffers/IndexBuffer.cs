using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class IndexBuffer : IBuffer
    {
        public uint BufferID { get; }

        public unsafe IndexBuffer(uint[] _indices)
        {
            BufferID = glGenBuffer();
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, BufferID);
            fixed (uint* i = &_indices[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, _indices.Length * sizeof(uint), i, GL_STATIC_DRAW);
        }

        public unsafe void UpdateData(uint[] _indices)
        {
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, BufferID);
            fixed (uint* v = &_indices[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, _indices.Length * sizeof(uint), v, GL_STATIC_DRAW);
        }

        public void Bind()
        {
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, BufferID);
        }

        public void Unbind()
        {
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public void Dispose()
        {
            glDeleteBuffer(BufferID);
        }
    }
}
