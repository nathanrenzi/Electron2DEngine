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
            if (!File.Exists(_textureName))
            {
                Debug.LogError($"File [ {_textureName} ] does not exist!");
                return null;
            }

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
            image.UnlockBits(data);

            return new Texture2D(handle, image.Width, image.Height);
        }

        public static Texture2DArray LoadArray(string _textureName, int _layers, bool _loadAsNonSRGBA)
        {
            if (!File.Exists(_textureName))
            {
                Debug.LogError($"File [ {_textureName} ] does not exist!");
                return null;
            }

            uint handle = glGenTexture();
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D_ARRAY, handle);

            using var image = new Bitmap(_textureName);
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            var data = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            glTexImage3D(GL_TEXTURE_2D_ARRAY, 0, _loadAsNonSRGBA ? GL_RGBA : GL_SRGB_ALPHA, image.Width, image.Height/_layers, _layers, 0, GL_BGRA, GL_UNSIGNED_BYTE, data.Scan0);

            glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_WRAP_S, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_WRAP_T, GL_REPEAT);
            image.UnlockBits(data);

            return new Texture2DArray(handle, image.Width, image.Height/_layers, _layers);
        }

        public static Texture2DArray LoadArray(string _textureName, int _spriteWidth, int _spriteHeight, bool _loadAsNonSRGBA)
        {
            if (!File.Exists(_textureName))
            {
                Debug.LogError($"File [ {_textureName} ] does not exist!");
                return null;
            }

            uint handle = glGenTexture();
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D_ARRAY, handle);

            // Spritesheet Image
            var spritesheetImage = new Bitmap(_textureName);
            spritesheetImage.RotateFlip(RotateFlipType.RotateNoneFlipY);

            if (_spriteWidth > spritesheetImage.Width || _spriteHeight > spritesheetImage.Height)
            {
                Debug.LogError($"TEXTURE FACTORY: Incorrect sprite size. Cannot convert spritesheet {_textureName} into texture array" +
                    $"\nInput Width: {_spriteWidth} Input Height: {_spriteHeight} Texture Full Width: {spritesheetImage.Width} Texture Full Height: {spritesheetImage.Height}");
                return null;
            }

            // Calculating # of loops
            int horizontalLoops = (int)MathF.Floor(spritesheetImage.Width / (float)_spriteWidth);
            int verticalLoops = (int)MathF.Floor(spritesheetImage.Height / (float)_spriteHeight);

            // Creating texture array in memory
            glTexImage3D(GL_TEXTURE_2D_ARRAY, 0, _loadAsNonSRGBA ? GL_RGBA : GL_SRGB_ALPHA, _spriteWidth, _spriteHeight, horizontalLoops*verticalLoops, 0, GL_BGRA, GL_UNSIGNED_BYTE, IntPtr.Zero);

            // Subbing in data from the spritesheet
            for (int y = 0; y < verticalLoops; y++)
            {
                for (int x = 0; x < horizontalLoops; x++)
                {
                    // Cloning each sprite from spritesheet into a bitmap
                    Bitmap b = spritesheetImage.Clone(new Rectangle(x * _spriteWidth, y * _spriteHeight, _spriteWidth, _spriteHeight), spritesheetImage.PixelFormat);
                    var data = b.LockBits(
                        new Rectangle(0, 0, b.Width, b.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

                    glTexSubImage3D(GL_TEXTURE_2D_ARRAY, 0, 0, 0, x + (y * horizontalLoops), b.Width, b.Height, 1, GL_BGRA, GL_UNSIGNED_BYTE, data.Scan0);

                    b.UnlockBits(data);
                    b.Dispose();
                }
            }

            spritesheetImage.Dispose();

            glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_WRAP_S, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_WRAP_T, GL_REPEAT);

            return new Texture2DArray(handle, _spriteWidth, _spriteHeight, horizontalLoops * verticalLoops);
        }

        public static Texture2DArray LoadArray(string[] _textureNames, bool _loadAsNonSRGBA)
        {
            if (!File.Exists(_textureNames[0]))
            {
                Debug.LogError($"File [ {_textureNames[0]} ] does not exist!");
                return null;
            }

            uint handle = glGenTexture();
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D_ARRAY, handle);

            // First image
            using var firstImage = new Bitmap(_textureNames[0]);
            firstImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
            var firstData = firstImage.LockBits(
                new Rectangle(0, 0, firstImage.Width, firstImage.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            glTexImage3D(GL_TEXTURE_2D_ARRAY, 0, _loadAsNonSRGBA ? GL_RGBA : GL_SRGB_ALPHA, firstImage.Width, firstImage.Height, _textureNames.Length, 0, GL_BGRA, GL_UNSIGNED_BYTE, IntPtr.Zero);
            glTexSubImage3D(GL_TEXTURE_2D_ARRAY, 0, 0, 0, 0, firstImage.Width, firstImage.Height, 1, GL_BGRA, GL_UNSIGNED_BYTE, firstData.Scan0);

            for (int i = 1; i < _textureNames.Length; i++)
            {
                using var image = new Bitmap(_textureNames[0]);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                var data = image.LockBits(
                    new Rectangle(0, 0, firstImage.Width, firstImage.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                if(image.Width != firstImage.Width || image.Height != firstImage.Height)
                {
                    Debug.LogError($"TEXTURE FACTORY: Cannot load image {_textureNames[i]}, size is not the same as the first image in the array.");
                    continue;
                }

                glTexSubImage3D(GL_TEXTURE_2D_ARRAY, 0, 0, 0, i, image.Width, image.Height, 1, GL_BGRA, GL_UNSIGNED_BYTE, data.Scan0);

                image.UnlockBits(data);
            }

            glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_WRAP_S, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D_ARRAY, GL_TEXTURE_WRAP_T, GL_REPEAT);
            firstImage.UnlockBits(firstData);

            return new Texture2DArray(handle, firstImage.Width, firstImage.Height, _textureNames.Length);
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
