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
        public bool UseLinearFiltering;
        // Add normal & roughness & specular & metallic maps here once they are needed
        private Material(Shader _shader, Texture2D _mainTexture, Texture2D _normalTexture, Color _mainColor, bool _useLinearFiltering, float _normalScale)
        {
            Shader = _shader;
            MainTexture = _mainTexture;
            NormalTexture = _normalTexture;
            MainColor = _mainColor;
            UseLinearFiltering = _useLinearFiltering;
            NormalScale = _normalScale;
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
        /// <param name="_skipShaderBinding">Whether or not the material should skip binding the shader.
        /// This should be set to true if you know the correct shader is in use already.</param>
        /// <param name="_skipTextureBinding">Whether or not the material should skip binding the texture.
        /// This should be set to true if you know the correct texture is in use already.</param>
        public void Use(bool _skipShaderBinding = false, bool _skipTextureBinding = false)
        {
            if (!_skipShaderBinding)
            {
                // If the last used shader is the same, skipping .Use()
                if (shaderInUse != Shader)
                {
                    Shader.Use();
                    shaderInUse = Shader;
                }
            }

            if(!_skipTextureBinding)
            {
                // If the last used texture is the same, skipping .Use()
                if (mainTextureInUse != MainTexture)
                {
                    MainTexture.Use(GL_TEXTURE0);   // Main textures get bound to texture slot 0
                    //mainTextureInUse = MainTexture;
                }

                if (normalTextureInUse != NormalTexture)
                {
                    NormalTexture.Use(GL_TEXTURE1); // Normals get bound to texture slot 1
                    //normalTextureInUse = NormalTexture;
                }
            }

            // RenderLayerManager handles checking to see if the correct filtering type is already being used.
            RenderLayerManager.SetTextureFiltering(UseLinearFiltering);

            Shader.SetColor("mainColor", MainColor);
            Shader.SetFloat("normalScale", NormalScale);
        }
    }
}
