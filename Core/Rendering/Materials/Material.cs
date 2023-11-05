using Electron2D.Core.Management;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;

using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class Material
    {
        private static Texture2D blankTexture = null;
        private static Texture2D blankNormal = null;

        public Shader Shader;
        private static Shader shaderInUse = null;

        public Texture2D MainTexture;
        private static Texture2D mainTextureInUse = null;

        public Texture2D NormalTexture;
        private static Texture2D normalTextureInUse = null;
        public float NormalScale;

        public Color MainColor;
        public bool UsingLinearFiltering { get; }

        private Material(Shader _shader, Texture2D _mainTexture, Texture2D _normalTexture, Color _mainColor, bool _useLinearFiltering, float _normalScale)
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
                    Console.WriteLine("Material: Failed to compile shader.");
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
        public static Material Create(Shader _shader, Texture2D _mainTexture = null, Texture2D _normalTexture = null, bool _useLinearFiltering = false, float _normalScale = 1)
            => Create(_shader, Color.White, _mainTexture, _normalTexture, _useLinearFiltering, _normalScale);
        public static Material Create(Shader _shader, Color _mainColor, Texture2D _mainTexture = null, Texture2D _normalTexture = null, bool _useLinearFiltering = false, float _normalScale = 1)
        {
            if (blankTexture == null)
                blankTexture = ResourceManager.Instance.LoadTexture("Core/Rendering/CoreTextures/BlankTexture.png");

            if (blankNormal == null)
                blankNormal = ResourceManager.Instance.LoadTexture("Core/Rendering/CoreTextures/BlankNormal.png", true);

            return new Material(_shader, (_mainTexture == null ? blankTexture : _mainTexture), (_normalTexture == null ? blankNormal : _normalTexture), _mainColor, _useLinearFiltering, _normalScale);
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
            Shader.SetColor("mainColor", MainColor);
            Shader.SetFloat("normalScale", NormalScale);
        }
    }
}
