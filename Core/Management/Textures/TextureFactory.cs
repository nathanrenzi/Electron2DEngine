using Electron2D.Core.Rendering;
using System.Drawing;
using System.Drawing.Imaging;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Management.Textures
{
    public static class TextureFactory
    {
        private static int textureCursor = 0;

        public static Texture2D Load(string _textureName)
        {
            uint handle = glGenTexture();
            int textureUnit = GL_TEXTURE0 + textureCursor;
            if(textureUnit > GL_TEXTURE31)
            {
                throw new Exception($"Exceeded maximum texture slots that OpenGL can natively support: {textureCursor}");
            }
            glActiveTexture(textureUnit);
            glBindTexture(GL_TEXTURE_2D, handle);
            using var image = new Bitmap(_textureName);
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            var data = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, image.Width, image.Height, 0, GL_BGRA, GL_UNSIGNED_BYTE, data.Scan0);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
            glGenerateMipmap(GL_TEXTURE_2D);
            textureCursor++;
            return new Texture2D(handle, image.Width, image.Height, textureUnit);
        }
    }
}
