using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electron2D.Core.Rendering.Shaders
{
    public class GlobalShaders
    {
        public static Shader DefaultTexture { get; private set; } = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultTexture.glsl"), true);
        public static Shader DefaultUserInterface { get; private set; } = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultUserInterface.glsl"), true);
        public static Shader DefaultVertex { get; private set; } = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultVertex.glsl"), true);
    }
}
