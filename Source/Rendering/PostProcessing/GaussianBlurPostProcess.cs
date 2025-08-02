using Electron2D.Rendering.Shaders;

namespace Electron2D.Rendering.PostProcessing
{
    public class GaussianBlurPostProcess : IPostProcess
    {
        public int KernelSize { get; set; }
        public float[] Kernel { get; private set; }
        public float Sigma { get; set; }
        public float BlurRadius { get; set; }

        private Shader _shader;
        private int _lastKernelSize = 0;
        private float _lastSigmaSize = 0;

        public GaussianBlurPostProcess(float blurRadius, float sigma = 0.7f, int kernelSize = 16)
        {
            BlurRadius = blurRadius;
            Sigma = sigma;
            KernelSize = kernelSize;
            _shader = new Shader(Shader.ParseShader(ResourceManager.GetEngineResourcePath("Shaders/PostProcessing/GaussianBlur.glsl")), true);
        }

        public int PostProcess(int signal, FrameBuffer readBuffer)
        {
            int retVal = 0;
            _shader.Use();
            if(signal == 0)
            {
                // Horizontal blur
                _shader.SetVector2("direction", new System.Numerics.Vector2(1, 0));
                retVal = 1;
            }
            else if(signal == 1)
            {
                // Vertical blur
                _shader.SetVector2("direction", new System.Numerics.Vector2(0, 1));
                retVal = 0;
            }

            if (_lastSigmaSize != Sigma || _lastKernelSize != KernelSize)
            {
                Kernel = new float[KernelSize * 2 + 1];
                float sum = 0;
                for(int x = -KernelSize; x <= KernelSize; x++)
                {
                    float sample = SampleGaussianEquation(x);
                    sum += sample;
                    Kernel[x + KernelSize] = sample;
                }
                for (int i = 0; i < Kernel.Length; i++)
                {
                    Kernel[i] /= sum;
                    _shader.SetFloat($"coeffs[{i}]", Kernel[i]);
                }
                _shader.SetInt("kernelSize", KernelSize);
                _lastKernelSize = KernelSize;
                _lastSigmaSize = Sigma;
            }
            _shader.SetFloat("blurRadius", BlurRadius);

            readBuffer.AttachedTexture.Use(OpenGL.GL.GL_TEXTURE0);

            return retVal;
        }

        private float SampleGaussianEquation(int x)
        {
            return MathF.Exp(-(x * x) / (2 * Sigma * Sigma)) / (MathF.PI * 2 * Sigma * Sigma);
        }
    }
}