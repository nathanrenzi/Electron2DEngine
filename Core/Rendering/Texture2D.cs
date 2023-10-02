using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class Texture2D : IDisposable
    {
        private bool _disposed;
        public uint Handle { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public static Texture2D Blank { get; set; } // Still need to implement

        public Texture2D(uint _handle)
        {
            Handle = _handle;
        }

        public Texture2D(uint _handle, int _width, int _height) : this(_handle)
        {
            Width = _width;
            Height = _height;
        }

        ~Texture2D()
        {
            Dispose(false);
        }

        public void Use(int _textureSlot)
        {
            glActiveTexture(_textureSlot);
            glBindTexture(GL_TEXTURE_2D, Handle);
        }

        public void Dispose(bool _disposing)
        {
            if(!_disposed)
            {
                glDeleteTexture(Handle);
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
