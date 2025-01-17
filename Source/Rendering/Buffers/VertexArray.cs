using static Electron2D.OpenGL.GL;

namespace Electron2D.Rendering
{
    public class VertexArray : IBuffer, IDisposable
    {
        public uint BufferID { get; }
        private bool isDisposed = false;

        public VertexArray()
        {
            BufferID = glGenVertexArray();
        }

        ~VertexArray()
        {
            Dispose();
        }

        public unsafe void AddBuffer(VertexBuffer _vertexBuffer, BufferLayout _bufferLayout)
        {
            Bind();
            _vertexBuffer.Bind();
            var elements = _bufferLayout.GetBufferElements();
            int offset = 0;
            for (int i = 0; i < elements.Count(); i++)
            {
                var currentElement = elements[i];
                glEnableVertexAttribArray((uint)i);
                glVertexAttribPointer((uint)i, currentElement.count, currentElement.type, currentElement.normalized, _bufferLayout.GetStride(), (void*)offset);
                offset += currentElement.count * Utilities.GetSizeOfVertexAttribPointerType(currentElement.type);
            }
        }

        public void Bind()
        {
            glBindVertexArray(BufferID);
        }

        public void Unbind()
        {
            glBindVertexArray(0);
        }

        public void Dispose()
        {
            if(!isDisposed)
            {
                isDisposed = true;
                glDeleteVertexArray(BufferID);
            }
        }
    }
}
