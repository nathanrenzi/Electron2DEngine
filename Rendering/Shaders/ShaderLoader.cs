using System.Reflection;

namespace OpenGLTest.Rendering.Shaders
{
    public static class ShaderLoader
    {
        private static string shadersPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Rendering\\Shaders\\Default\\");

        public static string GetShader(string _shaderFolderName, ShaderType _type)
        {
            string newPath = Path.Combine(shadersPath, _shaderFolderName);
            string returnShaderPath = Path.Combine(newPath, _type == ShaderType.SHADER_VERTEX ? "vertex.txt" : "fragment.txt");
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
    }

    public enum ShaderType
    {
        SHADER_VERTEX,
        SHADER_FRAGMENT
    }
}
