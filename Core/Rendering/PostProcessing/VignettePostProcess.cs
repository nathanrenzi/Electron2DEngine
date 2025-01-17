using Electron2D.Core.Rendering.Shaders;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Core.Rendering.PostProcessing
{
    public class VignettePostProcess : IPostProcess
    {
        public Color Color { get; set; }
        public float Softness { get; set; }
        public float Radius { get; set; }
        public float Roundness { get; set; }

        private Shader _shader;

        public VignettePostProcess(Color color, float softness, float radius, float roundness)
        {
            _shader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/PostProcessing/Vignette.glsl"), true);
            Color = color;
            Softness = softness;
            Radius = radius;
            Roundness = roundness;
        }

        public int PostProcess(int signal, FrameBuffer readBuffer)
        {
            _shader.Use();
            _shader.SetVector2("resolution",
                new Vector2(Display.WindowSize.X, Display.WindowSize.Y));
            _shader.SetColor("color", Color);
            _shader.SetFloat("softness", Softness);
            _shader.SetFloat("radius", Radius);
            _shader.SetFloat("roundness", Roundness);
            readBuffer.AttachedTexture.Use(OpenGL.GL.GL_TEXTURE0);
            return 0;
        }
    }
}