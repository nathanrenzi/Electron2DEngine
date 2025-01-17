namespace Electron2D.Rendering.Shaders
{
    public class GlobalShaders
    {
        public static Shader DefaultTexturedVertex { get; private set; } = new Shader(Shader.ParseShader("Resources/Built-In/Shaders/DefaultTexturedVertex.glsl"), true);
        public static Shader DefaultTexture { get; private set; } = new Shader(Shader.ParseShader("Resources/Built-In/Shaders/DefaultTexture.glsl"), true);
        public static Shader DefaultTextureArray { get; private set; } = new Shader(Shader.ParseShader("Resources/Built-In/Shaders/DefaultTextureArray.glsl"), true);
        public static Shader DefaultInterface { get; private set; } = new Shader(Shader.ParseShader("Resources/Built-In/Shaders/DefaultInterface.glsl"), true);
        public static Shader DefaultVertex { get; private set; } = new Shader(Shader.ParseShader("Resources/Built-In/Shaders/DefaultVertex.glsl"), true);
        public static Shader DefaultText { get; private set; } = new Shader(Shader.ParseShader("Resources/Built-In/Shaders/DefaultText.glsl"), true);
        public static Shader DefaultLit { get; private set; } = new Shader(Shader.ParseShader("Resources/Built-In/Shaders/DefaultLit.glsl"), true, new string[] {"lights"});
    }
}
