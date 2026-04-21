using Electron2D.Rendering.Shaders;
using System.Drawing;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Rendering
{
    public class Material : IDisposable
    {
        private static SharedResource<Texture2D> _blankTexture = null;
        private static SharedResource<Texture2D> _blankNormal = null;

        public SharedResource<Shader> Shader { get; }
        public SharedResource<Texture2D> MainTexture { get; }
        public SharedResource<Texture2D> NormalTexture { get; }
        public float NormalScale { get; set; }
        public Color MainColor { get; set; }
        public bool UsingLinearFiltering { get; }

        private Material(SharedResource<Shader> shader, SharedResource<Texture2D> mainTexture, SharedResource<Texture2D> normalTexture,
            Color mainColor, bool useLinearFiltering, float normalScale)
        {
            Shader = shader.AddRef();
            MainTexture = mainTexture.AddRef();
            NormalTexture = normalTexture.AddRef();
            MainColor = mainColor;
            UsingLinearFiltering = useLinearFiltering;
            NormalScale = normalScale;

            if (!shader.Value.Compiled)
            {
                shader.Value.Compile();
                shader.Value.Use();
                shader.Value.SetInt("mainTextureSampler", 0);
                shader.Value.SetInt("normalTextureSampler", 1);
            }

            MainTexture.Value.SetFilteringMode(UsingLinearFiltering);
            NormalTexture.Value.SetFilteringMode(UsingLinearFiltering);
        }

        public void Dispose()
        {
            Shader.Release();
            MainTexture.Release();
            NormalTexture.Release();
        }

        internal static void Initialize()
        {
            _blankTexture ??= Resources.GetTexture(Resources.GetEngineResourcePath("Textures/BlankTexture.png"));
            _blankNormal ??= Resources.GetTexture(Resources.GetEngineResourcePath("Textures/BlankNormal.png"), true);
        }

        internal static void Shutdown()
        {
            _blankTexture.Release();
            _blankNormal.Release();
        }

        #region Static Methods
        public static Material Create(SharedResource<Shader> shader, SharedResource<Texture2D> mainTexture = null,
            SharedResource<Texture2D> normalTexture = null, bool useLinearFiltering = false, float normalScale = 1)
            => Create(shader, Color.White, mainTexture, normalTexture, useLinearFiltering, normalScale);

        public static Material Create(SharedResource<Shader> shader, Color mainColor, SharedResource<Texture2D> mainTexture = null,
            SharedResource<Texture2D> normalTexture = null, bool useLinearFiltering = false, float normalScale = 1)
        {
            return new Material(shader, mainTexture ?? _blankTexture, normalTexture ?? _blankNormal,
                mainColor, useLinearFiltering, normalScale);
        }

        public static Material Create(Material materialToCopy)
        {
            return new Material(materialToCopy.Shader, materialToCopy.MainTexture, materialToCopy.NormalTexture,
                materialToCopy.MainColor, materialToCopy.UsingLinearFiltering, materialToCopy.NormalScale);
        }

        public static Material Create(Color color)
        {
            return Create(GlobalShaders.DefaultTexture, color);
        }

        public static Material CreateCircle(SharedResource<Shader> shader, Color color)
        {
            SharedResource<Texture2D> tex = Resources.GetTexture(Resources.GetEngineResourcePath("Textures/Circle.png"));
            Material mat = Create(shader, color, tex);
            tex.Release();
            return mat;
        }

        public static Material CreateCircle(Color color)
        {
            return CreateCircle(GlobalShaders.DefaultTexture, color);
        }

        public static Material CreateCircle(Color color, bool forInterface = false)
        {
            return CreateCircle(forInterface ? GlobalShaders.DefaultInterface : GlobalShaders.DefaultTexture, color);
        }

        public static Material CreateLit(Color mainColor, SharedResource<Texture2D> mainTexture = null,
            SharedResource<Texture2D> normalTexture = null, bool useLinearFiltering = false, float normalScale = 1)
        {
            return Create(GlobalShaders.DefaultLit, mainColor, mainTexture, normalTexture, useLinearFiltering, normalScale);
        }

        public static Material CreateLit(SharedResource<Texture2D> mainTexture, SharedResource<Texture2D> normalTexture = null,
            bool useLinearFiltering = false, float normalScale = 1)
        {
            return Create(GlobalShaders.DefaultLit, Color.White, mainTexture, normalTexture, useLinearFiltering, normalScale);
        }
        #endregion

        public void Use()
        {
            MainTexture.Value.Use(GL_TEXTURE0);
            NormalTexture.Value.Use(GL_TEXTURE1);

            Shader.Value.Use();
            Shader.Value.SetInt("mainTextureSampler", 0);
            Shader.Value.SetInt("normalTextureSampler", 1);
            Shader.Value.SetFloat("totalLayers", 0);
            Shader.Value.SetColor("mainColor", MainColor);
            Shader.Value.SetFloat("normalScale", NormalScale);
        }
    }
}