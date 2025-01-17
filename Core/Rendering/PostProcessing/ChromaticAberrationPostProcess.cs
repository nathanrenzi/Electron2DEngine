using Electron2D.Core.Rendering.Shaders;
using System.Numerics;

namespace Electron2D.Core.Rendering.PostProcessing
{
    public class ChromaticAberrationPostProcess : IPostProcess
    {
        public float Intensity { get; set; }
        public float RedOffset { get; set; }
        public float GreenOffset { get; set; }
        public float BlueOffset { get; set; }

        private Shader _shader;

        public ChromaticAberrationPostProcess(float intensity, float redOffset = -0.005f,
            float greenOffset = -0.01f, float blueOffset = -0.015f)
        {
            _shader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/PostProcessing/ChromaticAberration.glsl"), true);
            Intensity = intensity;
            RedOffset = redOffset;
            GreenOffset = greenOffset;
            BlueOffset = blueOffset;
        }

        public int PostProcess(int signal, FrameBuffer readBuffer)
        {
            _shader.Use();
            _shader.SetVector2("resolution",
                new Vector2(Program.Game.CurrentWindowWidth, Program.Game.CurrentWindowHeight));
            _shader.SetFloat("intensity", Intensity);
            _shader.SetFloat("redOffset", RedOffset);
            _shader.SetFloat("greenOffset", GreenOffset);
            _shader.SetFloat("blueOffset", BlueOffset);
            readBuffer.AttachedTexture.Use(OpenGL.GL.GL_TEXTURE0);
            return 0;
        }
    }
}