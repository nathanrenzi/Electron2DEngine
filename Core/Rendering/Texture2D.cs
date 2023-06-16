using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class Texture2D : IDisposable
    {
        private bool _disposed;
        public uint handle { get; private set; }

        public Texture2D(uint _handle)
        {
            handle = _handle;
        }

        ~Texture2D()
        {
            Dispose(false);
        }

        public void Use()
        {
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, handle);
        }

        public void Dispose(bool _disposing)
        {
            if(!_disposed)
            {
                glDeleteTexture(handle);
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
