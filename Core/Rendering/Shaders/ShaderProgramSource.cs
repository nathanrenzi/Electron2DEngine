using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electron2D.Core.Rendering.Shaders
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
