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
