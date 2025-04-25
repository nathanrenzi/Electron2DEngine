using Electron2D.Rendering.Shaders;
using Newtonsoft.Json;
using System.Drawing;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Rendering
{
    public class Material
    {
        private struct MaterialData
        {
            public bool MainTextureIsSRGBA;
            public bool MainTextureIsArray;
            public string MainTexturePath;
            public bool NormalTextureIsSRGBA;
            public bool NormalTextureIsArray;
            public string NormalTexturePath;
            public string[] ShaderUniformTags;
            public string ShaderPath;
            public float NormalScale;
            public Color MainColor;
            public bool UsingLinearFiltering;
        }

        private static Texture2D _blankTexture = null;
        private static Texture2D _blankNormal = null;

        public Shader Shader;
        public ITexture MainTexture;
        public ITexture NormalTexture;
        public float NormalScale;
        public Color MainColor;
        public bool UsingLinearFiltering { get; }

        private Material(Shader shader, ITexture mainTexture, ITexture normalTexture, Color mainColor, bool useLinearFiltering, float normalScale)
        {
            Shader = shader;
            MainTexture = mainTexture;
            NormalTexture = normalTexture;
            MainColor = mainColor;
            UsingLinearFiltering = useLinearFiltering;
            NormalScale = normalScale;

            // Compiling and setting up the shader if not done already.
            if (!shader.Compiled)
            {
                if(!shader.Compile())
                {
                    Debug.LogError("MATERIAL: Failed to compile shader.");
                    return;
                }

                shader.Use();
                shader.SetInt("mainTextureSampler", 0);
                shader.SetInt("normalTextureSampler", 1);
            }

            MainTexture.SetFilteringMode(UsingLinearFiltering);
            NormalTexture.SetFilteringMode(UsingLinearFiltering);
        }

        #region Static Methods
        public static Material Create(Shader shader, ITexture mainTexture = null, ITexture normalTexture = null, bool useLinearFiltering = false, float normalScale = 1)
            => Create(shader, Color.White, mainTexture, normalTexture, useLinearFiltering, normalScale);
        public static Material Create(Shader shader, Color mainColor, ITexture mainTexture = null, ITexture normalTexture = null, bool useLinearFiltering = false, float normalScale = 1)
        {
            if (_blankTexture == null)
                _blankTexture = ResourceManager.Instance.LoadTexture("Resources/Built-In/Textures/BlankTexture.png");

            if (_blankNormal == null)
                _blankNormal = ResourceManager.Instance.LoadTexture("Resources/Built-In/Textures/BlankNormal.png", true);

            return new Material(shader, (mainTexture == null ? _blankTexture : mainTexture), (normalTexture == null ? _blankNormal : normalTexture), mainColor, useLinearFiltering, normalScale);
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
        public static Material CreateCircle(Shader shader, Color color)
        {
            return Create(shader, color, ResourceManager.Instance.LoadTexture("Resources/Built-In/Textures/Circle.png"));
        }
        public static Material CreateCircle(Color color)
        {
            return CreateCircle(GlobalShaders.DefaultTexture, color);
        }
        public static Material CreateLit(Color mainColor, ITexture mainTexture = null, ITexture normalTexture = null,
            bool useLinearFiltering = false, float normalScale = 1)
        {
            Shader shader = GlobalShaders.DefaultLit;
            Material material = Create(shader, mainColor, mainTexture, normalTexture, useLinearFiltering, normalScale);
            return material;
        }
        public static Material CreateLit(ITexture mainTexture, ITexture normalTexture = null, bool useLinearFiltering = false, float normalScale = 1)
        {
            Shader shader = GlobalShaders.DefaultLit;
            Material material = Create(shader, Color.White, mainTexture, normalTexture, useLinearFiltering, normalScale);
            return material;
        }

        public static Material LoadFromJSON(string json)
        {
            MaterialData m = JsonConvert.DeserializeObject<MaterialData>(json);
            ITexture mainTexture, normalTexture;
            if(m.MainTextureIsArray)
            {
                Debug.LogError("Loading texture arrays in Material.Load() is not yet supported!");
                return null;
                //mainTexture = ResourceManager.Instance.LoadTextureArray(mdata.MainTexturePath);
            }
            else
            {
                mainTexture = ResourceManager.Instance.LoadTexture(m.MainTexturePath, !m.MainTextureIsSRGBA);
            }
            if (m.NormalTextureIsArray)
            {
                Debug.LogError("Loading texture arrays in Material.Load() is not yet supported!");
                return null;
                //normalTexture = ResourceManager.Instance.LoadTextureArray(mdata.NormalTexturePath);
            }
            else
            {
                normalTexture = ResourceManager.Instance.LoadTexture(m.NormalTexturePath, !m.NormalTextureIsSRGBA);
            }
            return Create(new Shader(Shader.ParseShader(m.ShaderPath), true, m.ShaderUniformTags), m.MainColor,
                mainTexture, normalTexture, m.UsingLinearFiltering, m.NormalScale);
        }
        #endregion

        /// <summary>
        /// Uses this material for rendering. Calls Shader.Use() and Texture2D.Use() internally.
        /// </summary>
        public void Use()
        {
            MainTexture.Use(GL_TEXTURE0);
            NormalTexture.Use(GL_TEXTURE1);

            Shader.Use();
            Shader.SetInt("mainTextureSampler", 0);
            Shader.SetInt("normalTextureSampler", 1);
            Shader.SetFloat("totalLayers", MainTexture.GetTextureLayers());
            Shader.SetColor("mainColor", MainColor);
            Shader.SetFloat("normalScale", NormalScale);
        }

        public string SaveAsJSON()
        {
            MaterialData m = new MaterialData();
            Texture2D texture = ResourceManager.Instance.TryGetTexture2DFromITexture(MainTexture);
            if(texture == null)
            {
                Debug.LogError("Loading texture arrays in Material.Save() is not yet supported!");
                return "";
            }
            m.MainTextureIsSRGBA = texture.IsSRGBA;
            m.MainTextureIsArray = false; // Temp while arrays are not supported
            m.MainTexturePath = texture.FilePath;
            Texture2D normal = ResourceManager.Instance.TryGetTexture2DFromITexture(MainTexture);
            if (normal == null)
            {
                Debug.LogError("Loading texture arrays in Material.Save() is not yet supported!");
                return "";
            }
            m.NormalTextureIsSRGBA = normal.IsSRGBA;
            m.NormalTextureIsArray = false;
            m.NormalTexturePath = normal.FilePath;
            m.ShaderUniformTags = Shader.GlobalUniformTags;
            m.ShaderPath = Shader.FilePath;
            m.NormalScale = NormalScale;
            m.MainColor = MainColor;
            m.UsingLinearFiltering = UsingLinearFiltering;

            return JsonConvert.SerializeObject(m);
        }
    }
}
