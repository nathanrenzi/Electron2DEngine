using Electron2D.Rendering.Shaders;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Rendering.PostProcessing
{
    public class LensDistortionPostProcess : IPostProcess
    {
        public float Intensity { get; set; }
        public Color ClearColor {  get; set; }

        private Shader _shader;

        /// <param name="intensity">The intensity of the warping.</param>
        /// <param name="clearColor">The color that is drawn to the screen if the UV goes out of bounds.</param>
        public LensDistortionPostProcess(float intensity, Color clearColor)
        {
            _shader = new Shader(Shader.ParseShader("Resources/Built-In/Shaders/PostProcessing/LensDistortion.glsl"), true);
            Intensity = intensity;
            ClearColor = clearColor;
        }

        /// <param name="intensity">The intensity of the warping.</param>
        public LensDistortionPostProcess(float intensity)
        {
            _shader = new Shader(Shader.ParseShader("Resources/Built-In/Shaders/PostProcessing/LensDistortion.glsl"), true);
            Intensity = intensity;
            ClearColor = Color.Black;
        }

        public int PostProcess(int signal, FrameBuffer readBuffer)
        {
            _shader.Use();
            _shader.SetVector2("resolution",
                new Vector2(Display.WindowSize.X, Display.WindowSize.Y));
            _shader.SetFloat("intensity", Intensity);
            _shader.SetColor("clearColor", ClearColor);
            readBuffer.AttachedTexture.Use(OpenGL.GL.GL_TEXTURE0);
            return 0;
        }
    }
}