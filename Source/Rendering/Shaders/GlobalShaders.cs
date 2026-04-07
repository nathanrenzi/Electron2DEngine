namespace Electron2D.Rendering.Shaders
{
    public class GlobalShaders
    {
        public static Shader TexturedVertex { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetEngineResourcePath("Shaders/TexturedVertex.glsl")), true);
        public static Shader Texture { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetEngineResourcePath("Shaders/Texture.glsl")), true);
        public static Shader TextureArray { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetEngineResourcePath("Shaders/TextureArray.glsl")), true);
        public static Shader Interface { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetEngineResourcePath("Shaders/Interface.glsl")), true);
        public static Shader Vertex { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetEngineResourcePath("Shaders/Vertex.glsl")), true);
        public static Shader Text { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetEngineResourcePath("Shaders/Text.glsl")), true);
        public static Shader Lit { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetEngineResourcePath("Shaders/Lit.glsl")), true, ["lights"]);
        public static Shader StencilOnly { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetEngineResourcePath("Shaders/StencilOnly.glsl")), true);
    }
}
