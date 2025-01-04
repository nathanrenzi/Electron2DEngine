using Electron2D.Core.Rendering.Shaders;

namespace Electron2D.Core.Rendering.PostProcessing
{
    public class InvertedPostProcess : IPostProcess
    {
        private Shader _shader;

        public InvertedPostProcess()
        {
            _shader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/PostProcessing/Inverted.glsl"), true);
        }

        public void PostProcess(FrameBuffer readBuffer)
        {
            _shader.Use();
            readBuffer.AttachedTexture.Use(OpenGL.GL.GL_TEXTURE0);
        }
    }
}
