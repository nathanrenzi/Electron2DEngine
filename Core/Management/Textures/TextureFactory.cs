using Electron2D.Core.Rendering;
using System.Drawing;
using System.Drawing.Imaging;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Management
{
    public static class TextureFactory
    {
        public static Texture2D Load(string _textureName, bool _loadAsNonSRGBA)
        {
            uint handle = glGenTexture();
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, handle);

            using var image = new Bitmap(_textureName);
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            var data = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
   
            glTexImage2D(GL_TEXTURE_2D, 0, _loadAsNonSRGBA ? GL_RGBA : GL_SRGB_ALPHA, image.Width, image.Height, 0, GL_BGRA, GL_UNSIGNED_BYTE, data.Scan0);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
            glGenerateMipmap(GL_TEXTURE_2D);
            image.UnlockBits(data);


            return new Texture2D(handle, image.Width, image.Height);
        }

        public static unsafe Texture2D Create(int _width, int _height)
        {
            uint handle = glGenTexture();
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, handle);
            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, _width, _height, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);

            return new Texture2D(handle, _width, _height);
        }
    }
}
