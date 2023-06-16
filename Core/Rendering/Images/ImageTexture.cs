using StbImageSharp;
using System.Reflection;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering.Images
{
    public class ImageTexture
    {
        public string path { get; private set; } = "";
        public ImageResult image { get; private set; }

        private string imagesPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Build\\Resources\\Images\\");

        public ImageTexture(string _path)
        {
            // Loading and generating texture
            path = Path.Combine(imagesPath, _path);
            byte[] data = File.ReadAllBytes(path);
            StbImage.stbi_set_flip_vertically_on_load(1);
            image = ImageResult.FromMemory(data, ColorComponents.RedGreenBlueAlpha);
        }

        /// <summary>
        /// Binds the texture to a specific texture slot
        /// </summary>
        /// <param name="_textureSlot">The slot to bind to: 0-16</param>
        public unsafe void Use(int _textureSlot)
        {
            fixed (byte* d = image.Data)
            {
                if (d != null)
                {
                    glActiveTexture(GL_TEXTURE0 + _textureSlot);
                    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, image.Width, image.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, d);
                    glGenerateMipmap(GL_TEXTURE_2D);
                }
                else
                {
                    Console.WriteLine("Failed to load texture.");
                }
            }
        }
    }
}