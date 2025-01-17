using static Electron2D.OpenGL.GL;

namespace Electron2D.Rendering
{
    public class BufferLayout
    {
        private List<BufferElement> elements = new();
        private int stride;
        private int rawStride;

        public BufferLayout()
        {
            stride = 0;
        }

        public List<BufferElement> GetBufferElements() => elements;
        public int GetStride() => stride;
        public int GetRawStride() => rawStride;

        public void Add<T>(int _count, bool _normalized = false) where T : struct
        {
            int pointerType;
            if(typeof(float) == typeof(T))
            {
                pointerType = GL_FLOAT;
                stride += sizeof(float) * _count;
                rawStride += _count;
            }
            else if(typeof(uint) == typeof(T))
            {
                pointerType = GL_UNSIGNED_INT;
                stride += sizeof(uint) * _count;
                rawStride += _count;
            }
            else if(typeof(byte) == typeof(T))
            {
                pointerType = GL_UNSIGNED_BYTE;
                stride += sizeof(byte) * _count;
                rawStride += _count;
            }
            else
            {
                throw new ArgumentException($"{typeof(T)} is not a valid type.");
            }

            elements.Add(new BufferElement() { type = pointerType, count = _count, normalized = _normalized });
        }
    }
}
