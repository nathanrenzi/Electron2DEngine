using Electron2D.Audio;
using Electron2D.Management;
using Electron2D.Rendering;
using Electron2D.Rendering.Text;
using System.Drawing;
using System.Text;

namespace Electron2D
{
    public sealed class ResourceManager
    {
        private static ResourceManager instance = null;
        private static readonly object loc = new();
        private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
        private Dictionary<string, Texture2DArray> textureArrayCache = new Dictionary<string, Texture2DArray>();
        private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
        private Dictionary<uint, Texture2D> textureHandleCache = new Dictionary<uint, Texture2D>();
        private Dictionary<FontArguments, FontGlyphStore> fontCache = new Dictionary<FontArguments, FontGlyphStore>();

        public static ResourceManager Instance
        {
            get
            {
                lock(loc)
                {
                    if(instance == null)
                    {
                        instance = new ResourceManager();
                    }
                    return instance;
                }
            }
        }

        private static bool ValidatePath(string path)
        {
            if(!File.Exists(path))
            {
                Debug.LogError($"File not found: {path}");
                return false;
            }

            return true;
        }

        public static string GetEngineResourcePath(string localPath)
        {
            return Path.Combine(ProjectSettings.EngineResourcePath, localPath);
        }

        public Texture2D TryGetTexture2DFromITexture(ITexture texture)
        {
            foreach (var tex in textureCache)
            {
                if(texture == tex.Value)
                {
                    return tex.Value;
                }
            }

            return null;
        }

        public Texture2DArray TryGetTextureArrayFromITexture(ITexture texture)
        {
            foreach (var tex in textureArrayCache)
            {
                if (texture == tex.Value)
                {
                    return tex.Value;
                }
            }

            return null;
        }

        #region Texture Arrays
        /// <summary>
        /// Removes a texture from the cache. This is called from <see cref="Texture2DArray.Dispose()"/>
        /// </summary>
        /// <param name="texture">The texture to remove from the cache.</param>
        public void RemoveTextureArray(Texture2DArray texture)
        {
            foreach (var pair in textureArrayCache)
            {
                if (pair.Value == texture)
                {
                    textureCache.Remove(pair.Key);
                }
            }
        }

        /// <summary>
        /// Directly loads a vertical spritesheet as a texture array, from bottom to top.
        /// </summary>
        /// <param name="textureFileName"></param>
        /// <returns></returns>
        public Texture2DArray LoadTextureArray(string textureFileName, int layers, bool nonColor = false)
        {
            if (!ValidatePath(textureFileName))
                return null;

            textureArrayCache.TryGetValue(textureFileName, out var value);
            if (value is not null)
            {
                return value;
            }

            value = TextureFactory.LoadArray(textureFileName, layers, nonColor);
            textureArrayCache.Add(textureFileName, value);
            return value;
        }

        /// <summary>
        /// Loads multiple individual textures into one texture array. All textures must be the same size.
        /// </summary>
        /// <param name="textureFileNames"></param>
        /// <param name="nonColor"></param>
        /// <returns></returns>
        public Texture2DArray LoadTextureArray(string[] textureFileNames, bool nonColor = false)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < textureFileNames.Length; i++)
            {
                if (!ValidatePath(textureFileNames[i]))
                    return null;
                builder.Append(textureFileNames[i]);
            }

            textureArrayCache.TryGetValue(builder.ToString(), out var value);
            if (value is not null)
            {
                return value;
            }

            value = TextureFactory.LoadArray(textureFileNames, nonColor);
            textureArrayCache.Add(builder.ToString(), value);
            return value;
        }

        /// <summary>
        /// Loads a spritesheet as a texture array, left to right, top to bottom.
        /// </summary>
        /// <param name="textureFileName"></param>
        /// <param name="spriteWidth"></param>
        /// <param name="spriteHeight"></param>
        /// <param name="nonColor"></param>
        /// <returns></returns>
        public Texture2DArray LoadTextureArray(string textureFileName, int spriteWidth,  int spriteHeight, bool nonColor = false)
        {
            if (!ValidatePath(textureFileName))
                return null;

            textureArrayCache.TryGetValue(textureFileName, out var value);
            if (value is not null)
            {
                return value;
            }

            value = TextureFactory.LoadArray(textureFileName, spriteWidth, spriteHeight, nonColor);
            textureArrayCache.Add(textureFileName, value);
            return value;
        }
        #endregion

        #region Textures
        /// <summary>
        /// Loads a texture into memory and returns it.
        /// After a texture is loaded, the already stored texture will be returned instead of creating a new Texture2D object.
        /// </summary>
        /// <param name="textureFileName">The local file path of the texture. Ex. Resources/Textures/TextureNameHere.png</param>
        /// <returns></returns>
        public Texture2D LoadTexture(string textureFileName, bool nonColor = false)
        {
            if (!ValidatePath(textureFileName))
                return null;

            textureCache.TryGetValue(textureFileName, out var value);
            if(value is not null)
            {
                return value;
            }

            value = TextureFactory.Load(textureFileName, nonColor);
            textureCache.Add(textureFileName, value);
            textureHandleCache.Add(value.Handle, value);
            return value;
        }

        /// <summary>
        /// Returns a previously loaded texture using it's OpenGL handle.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public Texture2D GetTexture(uint handle)
        {
            textureHandleCache.TryGetValue(handle, out var value);
            if (value is not null)
            {
                return value;
            }

            Debug.LogError($"Texture with handle {handle} does not exist.");
            return null;
        }

        /// <summary>
        /// Removes a texture from the cache. This is called from <see cref="Texture2D.Dispose()"/>
        /// </summary>
        /// <param name="texture">The texture to remove from the cache.</param>
        public void RemoveTexture(Texture2D texture)
        {
            textureHandleCache.Remove(texture.Handle);
            foreach (var pair in textureCache)
            {
                if(pair.Value == texture)
                {
                    textureCache.Remove(pair.Key);
                }
            }
        }

        /// <summary>
        /// Returns the size of a texture. Used by <see cref="ITexture2DManager"/>.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public Point GetTextureSize(object texture)
        {
            if(textureHandleCache.TryGetValue((uint)texture, out Texture2D tex))
            {
                return new Point(tex.Width, tex.Height);
            }
            else
            {
                Debug.LogError($"Texture handle {texture} is not registered, cannot get size.");
                return new Point(0, 0);
            }
        }

        /// <summary>
        /// Sets the pixel data of a texture.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="bounds"></param>
        /// <param name="data"></param>
        public void SetTextureData(object texture, Rectangle bounds, byte[] data)
        {
            if (textureHandleCache.TryGetValue((uint)texture, out Texture2D tex))
            {
                tex.SetData(bounds, data);
            }
            else
            {
                Debug.LogError($"Texture handle {texture} is not registered, cannot set data.");
            }
        }
        #endregion

        #region Fonts
        public FontGlyphStore LoadFont(string fontFile, int fontSize, float fontScale, int outlineSize)
        {
            if (!ValidatePath(fontFile))
                return null;

            string[] s = fontFile.Split('/');
            FontArguments args = new FontArguments() { FontFile = s[s.Length - 1], FontSize = fontSize,
                FontScale = fontScale, OutlineWidth = outlineSize };
            fontCache.TryGetValue(args, out var value);
            if (value is not null)
            {
                return value;
            }

            value = FontGlyphFactory.Load(fontFile, fontSize, fontScale, outlineSize);
            fontCache.Add(args, value);
            return value;
        }
        #endregion

        #region AudioClips
        public AudioClip LoadAudioClip(string audioFileName)
        {
            if (!ValidatePath(audioFileName))
                return null;

            audioClipCache.TryGetValue(audioFileName, out var value);
            if (value is not null)
            {
                return value;
            }

            value = new AudioClip(audioFileName);
            audioClipCache.Add(audioFileName, value);
            return value;
        }

        public void RemoveAudioClip(AudioClip _audioClip)
        {
            audioClipCache.Remove(_audioClip.FileName);
        }
        #endregion
    }
}
