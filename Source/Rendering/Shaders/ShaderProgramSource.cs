namespace Electron2D.Rendering.Shaders
{
    public class ShaderProgramSource
    {
        public string FilePath;
        public string VertexShaderSource;
        public string FragmentShaderSource;

        public ShaderProgramSource(string _filePath, string _vertexShaderSource, string _fragmentShaderSource)
        {
            FilePath = _filePath;
            VertexShaderSource = _vertexShaderSource;
            FragmentShaderSource = _fragmentShaderSource;
        }
    }
}
