using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electron2D.Core.Rendering.Shaders
{
    public class ShaderProgramSource
    {
        public string vertexShaderSource;
        public string fragmentShaderSource;

        public ShaderProgramSource(string _vertexShaderSource, string _fragmentShaderSource)
        {
            vertexShaderSource = _vertexShaderSource;
            fragmentShaderSource = _fragmentShaderSource;
        }
    }
}
