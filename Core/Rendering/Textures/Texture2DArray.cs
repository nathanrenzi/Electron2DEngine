using Electron2D.Core.Management;
using System.Drawing;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class Texture2DArray : ITexture, IDisposable
    {
        private bool _disposed;
        public uint Handle { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Layers { get; set; }
        public int GetTextureLayers() => Layers;

        public Texture2DArray(uint _handle, int _width, int _height, int _layers)
        {
            Handle = _handle;
            Width = _width;
            Height = _height;
            Layers = _layers;
        }

        ~Texture2DArray()
        {
            Dispose(false);
        }

        public void Use(int _textureSlot)
        {
            glActiveTexture(_textureSlot);
            glBindTexture(GL_TEXTURE_2D_ARRAY, Handle);
        }

        public void SetFilteringMode(bool _linear)
        {
            Use(GL_TEXTURE0);

            if (_linear)
            {
                // Linear filtering
                glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
                glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            }
            else
            {
                // Nearest filtering
                glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
                glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            }
        }

        public void Dispose(bool _disposing)
        {
            if (!_disposed)
            {
                glDeleteTexture(Handle);
                ResourceManager.Instance.RemoveTextureArray(this);
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
