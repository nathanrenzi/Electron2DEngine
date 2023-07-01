using Electron2D.Core.Rendering.Shaders;

namespace Electron2D.Core.Rendering
{
    public static class BatchManager
    {
        public static Batch staticObjectBatch { get; private set; } = new Batch(new Shader(Shader.ParseShader("Core/Rendering/Shaders/DefaultBatchTexture.glsl"), true), -1);
    }
}
