using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class ShaderStorageBuffer : IBuffer
    {
        public uint bufferID { get; }

        public ShaderStorageBuffer()
        {
            bufferID = glGenBuffer();
        }

        public void Bind()
        {
            
        }

        public void Unbind()
        {
            
        }
    }
}
