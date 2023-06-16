using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class BufferLayout
    {
        private List<BufferElement> elements = new();
        private int stride;

        public BufferLayout()
        {
            stride = 0;
        }

        public List<BufferElement> GetBufferElements() => elements;
        public int GetStride() => stride;

        public void Add<T>(int _count, bool _normalized = false) where T : struct
        {
            int pointerType;
            if(typeof(float) == typeof(T))
            {
                pointerType = GL_FLOAT;
                stride += sizeof(float) * _count;
            }
            else if(typeof(uint) == typeof(T))
            {
                pointerType = GL_UNSIGNED_INT;
                stride += sizeof(uint) * _count;
            }
            else if(typeof(byte) == typeof(T))
            {
                pointerType = GL_UNSIGNED_BYTE;
                stride += sizeof(byte) * _count;
            }
            else
            {
                throw new ArgumentException($"{typeof(T)} is not a valid type.");
            }

            elements.Add(new BufferElement() { type = pointerType, count = _count, normalized = _normalized });
        }
    }
}
