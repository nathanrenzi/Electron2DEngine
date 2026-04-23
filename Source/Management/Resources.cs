using Electron2D.Management;
using Electron2D.Rendering;
using Electron2D.Rendering.Text;
using System.Text;

namespace Electron2D
{
    public static class Resources
    {
        private static Dictionary<string, SharedResource<Texture2DArray>> textureArrayCache = new();
        private static Dictionary<string, SharedResource<Texture2D>> textureCache = new();
        private static Dictionary<uint, SharedResource<Texture2D>> textureHandleCache = new();
        private static Dictionary<FontArgs, SharedResource<FontGlyphStore>> fontCache = new();

        internal static void Shutdown()
        {
            foreach (var handle in textureCache.Values) handle.Release();
            foreach (var handle in textureArrayCache.Values) handle.Release();
            foreach (var handle in fontCache.Values) handle.Release();
            textureCache.Clear();
            textureArrayCache.Clear();
            textureHandleCache.Clear();
            fontCache.Clear();
        }

        private static bool ValidatePath(string path)
        {
            if (!File.Exists(path))
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

        #region Texture Arrays
        public static SharedResource<Texture2DArray> GetTextureArray(string path, int layers, bool nonColor = false)
        {
            if (!ValidatePath(path)) return null;
            if (!textureArrayCache.TryGetValue(path, out var handle))
            {
                handle = SharedResource<Texture2DArray>.Create(TextureFactory.LoadArray(path, layers, nonColor));
                textureArrayCache.Add(path, handle);
            }
            return handle.AddRef();
        }

        public static SharedResource<Texture2DArray> GetTextureArray(string[] paths, bool nonColor = false)
        {
            StringBuilder builder = new();
            foreach (var path in paths)
            {
                if (!ValidatePath(path)) return null;
                builder.Append(path);
            }

            string key = builder.ToString();
            if (!textureArrayCache.TryGetValue(key, out var handle))
            {
                handle = SharedResource<Texture2DArray>.Create(TextureFactory.LoadArray(paths, nonColor));
                textureArrayCache.Add(key, handle);
            }
            return handle.AddRef();
        }

        public static SharedResource<Texture2DArray> GetTextureArray(string path, int spriteWidth, int spriteHeight, bool nonColor = false)
        {
            if (!ValidatePath(path)) return null;
            if (!textureArrayCache.TryGetValue(path, out var handle))
            {
                handle = SharedResource<Texture2DArray>.Create(TextureFactory.LoadArray(path, spriteWidth, spriteHeight, nonColor));
                textureArrayCache.Add(path, handle);
            }
            return handle.AddRef();
        }
        #endregion

        #region Textures
        public static SharedResource<Texture2D> GetTexture(string path, bool nonColor = false)
        {
            if (!ValidatePath(path)) return null;
            if (!textureCache.TryGetValue(path, out var handle))
            {
                var texture = TextureFactory.Load(path, nonColor);
                handle = SharedResource<Texture2D>.Create(texture);
                textureCache.Add(path, handle);
                textureHandleCache.Add(texture.Handle, handle);
            }
            return handle.AddRef();
        }

        public static SharedResource<Texture2D> GetTexture(uint handle)
        {
            if (textureHandleCache.TryGetValue(handle, out var resource))
                return resource.AddRef();

            throw new InvalidOperationException($"Texture with handle {handle} does not exist.");
        }
        #endregion

        #region Fonts
        public static SharedResource<FontGlyphStore> GetFont(string path, int fontSize, float fontScale, int outlineSize)
        {
            if (!ValidatePath(path)) return null;

            string[] s = path.Split('/');
            FontArgs args = new FontArgs()
            {
                FontFile = s[s.Length - 1],
                FontSize = fontSize,
                FontScale = fontScale,
                OutlineWidth = outlineSize
            };

            if (!fontCache.TryGetValue(args, out var handle))
            {
                handle = SharedResource<FontGlyphStore>.Create(FontGlyphFactory.Load(path, fontSize, fontScale, outlineSize));
                fontCache.Add(args, handle);
            }
            return handle.AddRef();
        }
        #endregion
    }
}