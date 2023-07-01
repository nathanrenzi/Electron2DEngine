using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class IndexBuffer : IBuffer
    {
        public uint bufferID { get; }

        public unsafe IndexBuffer(uint[] _indices)
        {
            bufferID = glGenBuffer();
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, bufferID);
            fixed (uint* i = &_indices[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, _indices.Length * sizeof(uint), i, GL_STATIC_DRAW);
        }

        public unsafe void UpdateData(uint[] _vertices)
        {
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, bufferID);
            fixed (uint* v = &_vertices[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, _vertices.Length * sizeof(uint), v, GL_STATIC_DRAW);
        }

        public void Bind()
        {
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, bufferID);
        }

        public void Unbind()
        {
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }
    }
}
