using Electron2D.Core.Management.Textures;
using Electron2D.Core.Rendering;

namespace Electron2D.Core.Management
{
    public sealed class ResourceManager
    {
        private static ResourceManager instance = null;
        private static readonly object loc = new();
        private IDictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

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

        public Texture2D LoadTexture(string _textureName)
        {
            textureCache.TryGetValue(_textureName, out var value);
            if(value is not null)
            {
                return value;
            }

            value = TextureFactory.Load(_textureName);
            textureCache.Add(_textureName, value);
            return value;
        }
    }
}
