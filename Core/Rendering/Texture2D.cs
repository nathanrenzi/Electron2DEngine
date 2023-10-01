using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class Texture2D : IDisposable
    {
        private bool _disposed;
        public uint handle { get; private set; }
        public int width { get; set; }
        public int height { get; set; }
        public int textureSlot { get; set; } = GL_TEXTURE0; // See note below, might want to remove in favor of material system

        public Texture2D(uint _handle)
        {
            handle = _handle;
        }

        public Texture2D(uint _handle, int _width, int _height) : this(_handle)
        {
            width = _width;
            height = _height;
        }

        public Texture2D(uint _handle, int _width, int _height,  int _textureSlot) : this(_handle, _width, _height)
        {
            textureSlot = _textureSlot;
        }

        ~Texture2D()
        {
            Dispose(false);
        }

        public void Use() => Use(textureSlot); // Might want to remove default texture slot, since material system is replacing that
        public void Use(int _textureSlot)
        {
            glActiveTexture(_textureSlot);
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
