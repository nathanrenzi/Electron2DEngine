namespace Electron2D.Rendering.Shaders
{
    public static class GlobalShaders
    {
        public static SharedResource<Shader> DefaultTexturedVertex { get; private set; }
        public static SharedResource<Shader> DefaultTexture { get; private set; }
        public static SharedResource<Shader> DefaultTextureArray { get; private set; }
        public static SharedResource<Shader> DefaultInterface { get; private set; }
        public static SharedResource<Shader> DefaultVertex { get; private set; }
        public static SharedResource<Shader> DefaultText { get; private set; }
        public static SharedResource<Shader> DefaultLit { get; private set; }

        internal static void Initialize()
        {
            DefaultTexturedVertex = SharedResource<Shader>.Create(new Shader(Shader.ParseShader(Resources.GetEngineResourcePath("Shaders/DefaultTexturedVertex.glsl")), true));
            DefaultTexture = SharedResource<Shader>.Create(new Shader(Shader.ParseShader(Resources.GetEngineResourcePath("Shaders/DefaultTexture.glsl")), true));
            DefaultTextureArray = SharedResource<Shader>.Create(new Shader(Shader.ParseShader(Resources.GetEngineResourcePath("Shaders/DefaultTextureArray.glsl")), true));
            DefaultInterface = SharedResource<Shader>.Create(new Shader(Shader.ParseShader(Resources.GetEngineResourcePath("Shaders/DefaultInterface.glsl")), true));
            DefaultVertex = SharedResource<Shader>.Create(new Shader(Shader.ParseShader(Resources.GetEngineResourcePath("Shaders/DefaultVertex.glsl")), true));
            DefaultText = SharedResource<Shader>.Create(new Shader(Shader.ParseShader(Resources.GetEngineResourcePath("Shaders/DefaultText.glsl")), true));
            DefaultLit = SharedResource<Shader>.Create(new Shader(Shader.ParseShader(Resources.GetEngineResourcePath("Shaders/DefaultLit.glsl")), true, new string[] { "lights" }));
        }

        internal static void Shutdown()
        {
            DefaultTexturedVertex.Release();
            DefaultTexture.Release();
            DefaultTextureArray.Release();
            DefaultInterface.Release();
            DefaultVertex.Release();
            DefaultText.Release();
            DefaultLit.Release();
        }
    }
}