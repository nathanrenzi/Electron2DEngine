using System.Reflection;

namespace OpenGLTest.Rendering.Shaders
{
    public static class ShaderLoader
    {
        private static Dictionary<DefaultShaderType, Shader> defaultShaders = new Dictionary<DefaultShaderType, Shader>();
        private static string shadersPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Rendering\\Shaders\\Default\\");

        /// <summary>
        /// Creates all default shader objects and adds them to the default shader list
        /// </summary>
        public static void Initialize()
        {
            string vertex = GetShaderCode("SpriteShader", ShaderCodeType.SHADER_VERTEX);
            string fragment = GetShaderCode("SpriteShader", ShaderCodeType.SHADER_FRAGMENT);

            if (vertex != "" && fragment != "")
            {
                Shader spriteShader = new Shader(vertex, fragment);
                spriteShader.Load();
                defaultShaders.Add(DefaultShaderType.SPRITE, spriteShader);
            }
        }

        /// <summary>
        /// Returns the specified type of shader code for a specific shader
        /// </summary>
        /// <param name="_shaderFolderName">The name of the shader folder</param>
        /// <param name="_type">The type of shader code to return</param>
        /// <returns>Shader code</returns>
        public static string GetShaderCode(string _shaderFolderName, ShaderCodeType _type)
        {
            string newPath = Path.Combine(shadersPath, _shaderFolderName);
            string returnShaderPath = Path.Combine(newPath, _type == ShaderCodeType.SHADER_VERTEX ? "vertex.txt" : "fragment.txt");
            if(File.Exists(returnShaderPath))
            {
                return File.ReadAllText(returnShaderPath);
            }
            else
            {
                Console.WriteLine("Error loading shader: " + _shaderFolderName);
                return "";
            }
        }

        /// <summary>
        /// Returns a shader specified by the default shader type
        /// </summary>
        /// <param name="_type">The shader type to return</param>
        /// <returns>A default shader</returns>
        public static Shader GetShader(DefaultShaderType _type)
        {
            if(defaultShaders.ContainsKey(_type))
            {
                return defaultShaders[_type];
            }
            else
            {
                return null;
            }
        }
    }

    public enum ShaderCodeType
    {
        SHADER_VERTEX,
        SHADER_FRAGMENT
    }

    public enum DefaultShaderType
    {
        SPRITE
    }
}
