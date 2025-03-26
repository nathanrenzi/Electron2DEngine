using Electron2D.Rendering.Shaders;
using System.Drawing;

namespace Electron2D.Rendering.PostProcessing
{
    public class ColorGradingPostProcess : IPostProcess
    {
        /// <summary>
        /// The color that the screen will be tinted. Set to white for no tint
        /// </summary>
        public Color ColorFilter { get; set; }
        /// <summary>
        /// Adjust the hue of all colors. [-1 - 1]
        /// </summary>
        public float HueShift { get; set; }
        /// <summary>
        /// Adjust the intensity of all colors. [-1 = -100%, 0 = 100%, 1 = 200%, etc.]
        /// </summary>
        public float Saturation { get; set; }
        /// <summary>
        /// Adjust the brightness of the image. [-1 - 1]
        /// </summary>
        public float Brightness { get; set; }
        /// <summary>
        /// Adjust the overall range of tonal values.
        /// </summary>
        public float Contrast { get; set; }
        /// <summary>
        /// Set the white balance to a custom color temperature. [-1 - 1]
        /// </summary>
        public float Temperature { get; set; }

        private Shader _shader;

        /// <param name="colorFilter">The color that the screen will be tinted. Set to white for no tint.</param>
        /// <param name="hueShift">Adjust the hue of all colors. [-1 - 1]</param>
        /// <param name="saturation">Adjust the intensity of all colors. [-1 = -100%, 0 = 100%, 1 = 200%, etc.]</param>
        /// <param name="brightness">Adjust the brightness of the image. [-1 = -100%, 0 = 100%, 1 = 200%, etc.]</param>
        /// <param name="contrast">Adjust the overall range of tonal values. [-1 - 1]</param>
        /// <param name="temperature">Set the white balance to a custom color temperature. [-1 - 1]</param>
        public ColorGradingPostProcess(Color colorFilter, float hueShift = 0, float saturation = 0,
            float brightness = 0, float contrast = 0, float temperature = 0)
        {
            _shader = new Shader(Shader.ParseShader(ResourceManager.GetResourcePath("Shaders/PostProcessing/ColorGrading.glsl")), true);
            ColorFilter = colorFilter;
            HueShift = hueShift;
            Saturation = saturation;
            Brightness = brightness;
            Contrast = contrast;
            Temperature = temperature;
        }

        public int PostProcess(int signal, FrameBuffer readBuffer)
        {
            _shader.Use();
            _shader.SetColor("colorFilter", ColorFilter);
            _shader.SetFloat("hueShift", HueShift);
            _shader.SetFloat("saturation", Saturation);
            _shader.SetFloat("brightness", Brightness);
            _shader.SetFloat("contrast", Contrast);
            _shader.SetFloat("temperature", Temperature);
            readBuffer.AttachedTexture.Use(OpenGL.GL.GL_TEXTURE0);
            return 0;
        }
    }
}