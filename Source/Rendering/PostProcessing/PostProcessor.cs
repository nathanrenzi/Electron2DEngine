using Electron2D.Rendering.Shaders;
using System.Drawing;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Rendering.PostProcessing
{
    /// <summary>
    /// Manages all post processing effects and renders them to the screen.
    /// </summary>
    public class PostProcessor
    {
        public static PostProcessor Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new PostProcessor();
                }
                return _instance;
            }
        }
        private static PostProcessor _instance;

        private List<PostProcessingStack> _stacks = new List<PostProcessingStack>();

        // Used to write frame data to (faster)
        private FrameBuffer _renderBuffer;

        // Used to create a texture from frame data and swap between for multiple post processing effects
        private FrameBuffer _frameBuffer1;
        private FrameBuffer _frameBuffer2;

        private Shader _shader;

        private float[] _vertices = new float[]
        {
            -1.0f, -1.0f, 0.0f, 0.0f,
            1.0f, 1.0f, 1.0f, 1.0f,
            -1.0f, 1.0f, 0.0f, 1.0f,

            -1.0f, -1.0f, 0.0f, 0.0f,
            1.0f, -1.0f, 1.0f, 0.0f,
            1.0f, 1.0f, 1.0f, 1.0f
        };
        private VertexBuffer _vertexBuffer;
        private VertexArray _vertexArray;
        private bool _readingBuffer1 = false;
        private bool _initialized = false;

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _shader = new Shader(Shader.ParseShader(ProjectSettings.GetEngineResourcePath("Shaders/PostProcessing/DefaultPostProcessing.glsl")), true);

            _renderBuffer = new FrameBuffer(4, GL_RGB, GL_COLOR_ATTACHMENT0, false, true);
            _frameBuffer1 = new FrameBuffer(0, 0, 0, true, false);
            _frameBuffer2 = new FrameBuffer(0, 0, 0, true, false);
            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            _vertexArray = new VertexArray();
            _vertexBuffer = new VertexBuffer(_vertices);
            BufferLayout layout = new BufferLayout();
            layout.Add<float>(2); // Position
            layout.Add<float>(2); // UV
            _vertexArray.AddBuffer(_vertexBuffer, layout);
        }

        public void BeforeGameRender()
        {
            if (!_initialized) return;

            _renderBuffer.Bind();
            Color clearColor = Program.Game.BackgroundColor;
            glClearColor(clearColor.R / 255f, clearColor.G / 255f, clearColor.B / 255f, clearColor.A / 255f);
            glClear(GL_COLOR_BUFFER_BIT);
        }

        public void AfterGameRender()
        {
            if (!_initialized) return;

            int width = (int)Display.WindowSize.X;
            int height = (int)Display.WindowSize.Y;
            _renderBuffer.BindRead();
            _frameBuffer1.BindWrite();
            _readingBuffer1 = true;
            glBlitFramebuffer(0, 0, width, height, 0, 0, width, height, GL_COLOR_BUFFER_BIT, GL_NEAREST);
            glBindFramebuffer(GL_FRAMEBUFFER, 0); // Binds both READ and WRITE framebuffer to default framebuffer
        }

        public void Render()
        {
            if (!_initialized) return;

            _vertexArray.Bind();
            for (int s = 0; s < _stacks.Count; s++)
            {
                for (int i = 0; i < _stacks[s].Size; i++)
                {
                    int signal = 0;
                    do
                    {
                        if (_readingBuffer1)
                        {
                            _frameBuffer1.BindRead();
                            _frameBuffer2.BindWrite();
                            signal = _stacks[s].Get(i).PostProcess(signal, _frameBuffer1);
                            glDrawArrays(GL_TRIANGLES, 0, 6);
                        }
                        else
                        {
                            _frameBuffer1.BindWrite();
                            _frameBuffer2.BindRead();
                            signal = _stacks[s].Get(i).PostProcess(signal, _frameBuffer2);
                            glDrawArrays(GL_TRIANGLES, 0, 6);
                        }
                        _readingBuffer1 = !_readingBuffer1;
                    } while (signal != 0);
                }
            }
            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            _shader.Use();
            if(_readingBuffer1)
            {
                _frameBuffer1.AttachedTexture.Use(GL_TEXTURE0);
            }
            else
            {
                _frameBuffer2.AttachedTexture.Use(GL_TEXTURE0);
            }
          
            glDrawArrays(GL_TRIANGLES, 0, 6);
            _vertexArray.Unbind();
        }

        public void Register(PostProcessingStack stack)
        {
            if(!_initialized)
            {
                _initialized = true;
                Initialize();
            }

            bool inserted = false;
            for (int i = 0; i < _stacks.Count; i++)
            {
                if (_stacks[i].Priority > stack.Priority)
                {
                    continue;
                }
                else if (_stacks[i].Priority < stack.Priority)
                {
                    _stacks.Insert(i, stack);
                    inserted = true;
                    break;
                }
            }

            if(!inserted)
            {
                _stacks.Add(stack);
            }
        }

        public void Unregister(PostProcessingStack stack)
        {
            _stacks.Remove(stack);
        }
    }
}
