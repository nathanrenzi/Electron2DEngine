using Electron2D.Rendering.Shaders;
using System.Numerics;

namespace Electron2D.Rendering.PostProcessing
{
    public class FXAAPostProcess : IPostProcess
    {
        private Shader _shader;

        public FXAAPostProcess()
        {
            _shader = new Shader(Shader.ParseShader(ResourceManager.GetEngineResourcePath("Shaders/PostProcessing/FXAA.glsl")), true);
        }

        public int PostProcess(int signal, FrameBuffer readBuffer)
        {
            _shader.Use();
            _shader.SetVector2("texel",
                new Vector2(1f / Display.WindowSize.X, 1f / Display.WindowSize.Y));
            readBuffer.AttachedTexture.Use(OpenGL.GL.GL_TEXTURE0);
            return 0;
        }
    }
}
