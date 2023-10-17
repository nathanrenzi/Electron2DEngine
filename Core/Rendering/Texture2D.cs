using Electron2D.Core.Management;
using System.Drawing;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class Texture2D : IDisposable
    {
        private bool _disposed;
        public uint Handle { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Texture2D(uint _handle, int _width, int _height)
        {
            Handle = _handle;
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

        public unsafe void SetData(Rectangle _bounds, byte[] _data)
        {
            Use(GL_TEXTURE0);
            fixed(byte* ptr = _data)
            {
                glTexSubImage2D(GL_TEXTURE_2D, 0, _bounds.Left, _bounds.Top, _bounds.Width, _bounds.Height, GL_BGRA, GL_UNSIGNED_BYTE, new IntPtr(ptr));
            }
        }

        public void Dispose(bool _disposing)
        {
            if(!_disposed)
            {
                glDeleteTexture(Handle);
                ResourceManager.Instance.RemoveTexture(this);
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
