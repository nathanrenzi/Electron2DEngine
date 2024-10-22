using Electron2D.Core.Management;
using System.Drawing;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class Texture2D : ITexture, IDisposable
    {
        private bool _disposed;
        public uint Handle { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string FilePath { get; }
        public bool IsSRGBA { get; }

        public Texture2D(uint _handle, int _width, int _height, string _filePath = "", bool _isSRGBA = true)
        {
            Handle = _handle;
            Width = _width;
            Height = _height;
            FilePath = _filePath;
            IsSRGBA = _isSRGBA;
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
            // Using not in-use texture slot since this does not cache itself as the last used texture anywhere
            Use(GL_TEXTURE0);
            fixed (byte* ptr = _data)
            {
                glTexSubImage2D(GL_TEXTURE_2D, 0, _bounds.Left, _bounds.Top, _bounds.Width, _bounds.Height, GL_RGBA, GL_UNSIGNED_BYTE, ptr);
            }
        }

        public void SetFilteringMode(bool _linear)
        {
            Use(GL_TEXTURE0);

            if (_linear)
            {
                // Linear filtering
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            }
            else
            {
                // Nearest filtering
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            }
        }

        private void Dispose(bool _disposing)
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
