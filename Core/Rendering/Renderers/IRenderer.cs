using Electron2D.Core.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electron2D.Core.Rendering
{
    public interface IRenderer
    {
        public void Load();
        public void Render();
        public void SetVertexValueAll(int _type, float _value);
        public Shader GetShader();
    }
}
