using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electron2D.Core.Rendering.Shaders
{
    public class GlobalShaders
    {
        public static Shader DefaultTexturedVertex { get; private set; } = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultTexturedVertex.glsl"), true);
        public static Shader DefaultTexture { get; private set; } = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultTexture.glsl"), true);
        public static Shader DefaultTextureArray { get; private set; } = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultTextureArray.glsl"), true);
        public static Shader DefaultInterface { get; private set; } = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultInterface.glsl"), true);
        public static Shader DefaultVertex { get; private set; } = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultVertex.glsl"), true);
        public static Shader DefaultText { get; private set; } = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultText.glsl"), true);
        public static Shader DefaultLit { get; private set; } = new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultLit.glsl"), true, new string[] {"lights"});
    }
}
