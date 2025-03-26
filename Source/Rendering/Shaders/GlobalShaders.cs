namespace Electron2D.Rendering.Shaders
{
    public class GlobalShaders
    {
        public static Shader DefaultTexturedVertex { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetResourcePath("Shaders/DefaultTexturedVertex.glsl")), true);
        public static Shader DefaultTexture { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetResourcePath("Shaders/DefaultTexture.glsl")), true);
        public static Shader DefaultTextureArray { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetResourcePath("Shaders/DefaultTextureArray.glsl")), true);
        public static Shader DefaultInterface { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetResourcePath("Shaders/DefaultInterface.glsl")), true);
        public static Shader DefaultVertex { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetResourcePath("Shaders/DefaultVertex.glsl")), true);
        public static Shader DefaultText { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetResourcePath("Shaders/DefaultText.glsl")), true);
        public static Shader DefaultLit { get; private set; } = new Shader(Shader.ParseShader(ResourceManager.GetResourcePath("Shaders/DefaultLit.glsl")), true, new string[] {"lights"});
    }
}
