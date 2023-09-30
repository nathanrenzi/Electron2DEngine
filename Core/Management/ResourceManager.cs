using Electron2D.Core.Audio;
using Electron2D.Core.Management.Textures;
using Electron2D.Core.Rendering;

namespace Electron2D.Core.Management
{
    // MAKE LOADING HAPPEN DURING THE LOADING PHASE USING A MAP FILE THAT CONTAINS ALL SOUNDS AND TEXTURES THAT WILL NEED TO BE LOADED
    // THERE IS CURRENTLY LAG HAPPENING WHEN TEXTURES AND ESPECIALLY SOUNDS ARE LOADED IN AT RUNTIME
    public sealed class ResourceManager
    {
        private static ResourceManager instance = null;
        private static readonly object loc = new();
        private IDictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
        private IDictionary<string, CachedSound> soundCache = new Dictionary<string, CachedSound>();

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

        /// <summary>
        /// Loads a texture into memory and returns it.
        /// After a texture is loaded, the already stored texture will be returned instead of creating a new Texture2D object.
        /// </summary>
        /// <param name="_textureFileName">The local file path of the sound. Ex. Build/Resources/Textures/TextureNameHere.png</param>
        /// <returns></returns>
        public Texture2D LoadTexture(string _textureFileName)
        {
            textureCache.TryGetValue(_textureFileName, out var value);
            if(value is not null)
            {
                return value;
            }

            value = TextureFactory.Load(_textureFileName);
            textureCache.Add(_textureFileName, value);
            return value;
        }

        /// <summary>
        /// Gets a texture from the texture cache using the texture slot instead of file directory.
        /// </summary>
        /// <param name="_textureSlot">The texture slot of the texture (Must already be loaded)</param>
        /// <returns></returns>
        public Texture2D GetTexture(int _textureSlot)
        {
            foreach (var texture in textureCache.Values)
            {
                if (texture.textureSlot == _textureSlot)
                {
                    return texture;
                }
            }

            return textureCache.Values.First();
        }

        /// <summary>
        /// Loads a sound into memory and returns it.
        /// After a sound is loaded, the already stored sound will be returned instead of creating a new CachedSound object.
        /// </summary>
        /// <param name="_soundFileName">The local file path of the sound. Ex. Build/Resources/Audio/SFX/SoundFileNameHere.mp3</param>
        /// <returns></returns>
        public CachedSound LoadSound(string _soundFileName)
        {
            soundCache.TryGetValue(_soundFileName, out var value);
            if(value is not null)
            {
                return value;
            }

            value = new CachedSound(_soundFileName);
            soundCache.Add(_soundFileName, value);
            return value;
        }
    }
}
