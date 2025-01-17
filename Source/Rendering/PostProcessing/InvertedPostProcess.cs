using Electron2D.Rendering.Shaders;

namespace Electron2D.Rendering.PostProcessing
{
    public class InvertedPostProcess : IPostProcess
    {
        private Shader _shader;

        public InvertedPostProcess()
        {
            _shader = new Shader(Shader.ParseShader("Resources/Built-In/Shaders/PostProcessing/Inverted.glsl"), true);
        }

        public int PostProcess(int signal, FrameBuffer readBuffer)
        {
            _shader.Use();
            readBuffer.AttachedTexture.Use(OpenGL.GL.GL_TEXTURE0);
            return 0;
        }
    }
}
