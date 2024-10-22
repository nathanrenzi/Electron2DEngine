using Electron2D.Core.Management;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;
using Newtonsoft.Json;

using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
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

        private static Texture2D blankTexture = null;
        private static Texture2D blankNormal = null;

        public Shader Shader;

        public ITexture MainTexture;

        public ITexture NormalTexture;
        public float NormalScale;

        public Color MainColor;
        public bool UsingLinearFiltering { get; }

        private Material(Shader _shader, ITexture _mainTexture, ITexture _normalTexture, Color _mainColor, bool _useLinearFiltering, float _normalScale)
        {
            Shader = _shader;
            MainTexture = _mainTexture;
            NormalTexture = _normalTexture;
            MainColor = _mainColor;
            UsingLinearFiltering = _useLinearFiltering;
            NormalScale = _normalScale;

            // Compiling and setting up the shader if not done already.
            if (!_shader.Compiled)
            {
                if(!_shader.Compile())
                {
                    Debug.LogError("MATERIAL: Failed to compile shader.");
                    return;
                }

                _shader.Use();
                _shader.SetInt("mainTextureSampler", 0);
                _shader.SetInt("normalTextureSampler", 1);
            }

            MainTexture.SetFilteringMode(UsingLinearFiltering);
            NormalTexture.SetFilteringMode(UsingLinearFiltering);
        }

        #region Static Methods
        public static Material Create(Shader _shader, ITexture _mainTexture = null, ITexture _normalTexture = null, bool _useLinearFiltering = false, float _normalScale = 1)
            => Create(_shader, Color.White, _mainTexture, _normalTexture, _useLinearFiltering, _normalScale);
        public static Material Create(Shader _shader, Color _mainColor, ITexture _mainTexture = null, ITexture _normalTexture = null, bool _useLinearFiltering = false, float _normalScale = 1)
        {
            if (blankTexture == null)
                blankTexture = ResourceManager.Instance.LoadTexture("Core/Rendering/CoreTextures/BlankTexture.png");

            if (blankNormal == null)
                blankNormal = ResourceManager.Instance.LoadTexture("Core/Rendering/CoreTextures/BlankNormal.png", true);

            return new Material(_shader, (_mainTexture == null ? blankTexture : _mainTexture), (_normalTexture == null ? blankNormal : _normalTexture), _mainColor, _useLinearFiltering, _normalScale);
        }
        public static Material Create(Material _materialToCopy)
        {
            return new Material(_materialToCopy.Shader, _materialToCopy.MainTexture, _materialToCopy.NormalTexture,
                _materialToCopy.MainColor, _materialToCopy.UsingLinearFiltering, _materialToCopy.NormalScale);
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
