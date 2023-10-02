using Electron2D.Core.Rendering.Shaders;
using System.Drawing;

using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class Material
    {
        // The last used shader by the material class - Will not call Shader.Use() again if the correct one is already in use.
        private static Shader shaderInUse = null;
        private static Texture2D textureInUse = null; // Replace with texture-sorted rendering system

        public Shader Shader;
        public Texture2D MainTexture;
        public Color MainColor;
        public bool UseLinearFiltering;
        // Add normal & roughness maps here once they are needed

        private Material(Shader _shader, Texture2D _mainTexture, Color _mainColor, bool _useLinearFiltering = false)
        {
            Shader = _shader;
            MainTexture = _mainTexture;
            MainColor = _mainColor;
            UseLinearFiltering = _useLinearFiltering;
        }

        #region Static Methods
        public static Material Create(Shader _shader) => Create(_shader, null, Color.White);
        public static Material Create(Shader _shader, Texture2D _mainTexture, bool _useLinearFiltering = false) => Create(_shader, _mainTexture, Color.White, _useLinearFiltering);
        public static Material Create(Shader _shader, Texture2D _mainTexture, Color _mainColor, bool _useLinearFiltering = false)
        {
            return new Material(_shader, _mainTexture, _mainColor, _useLinearFiltering);
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
                    //shaderInUse = Shader;
                }
            }

            if(!_skipTextureBinding)
            {
                // If the last used texture is the same, skipping .Use()
                if (MainTexture != null && textureInUse != MainTexture)
                {
                    MainTexture.Use(GL_TEXTURE0);
                    //textureInUse = MainTexture;
                }
                else if(MainTexture == null)
                {

                }
            }

            // RenderLayerManager handles checking to see if the correct filtering type is already being used.
            RenderLayerManager.SetTextureFiltering(UseLinearFiltering);

            Shader.SetColor("mainColor", MainColor);
        }
    }
}
