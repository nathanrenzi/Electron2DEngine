using Electron2D.Core.Management;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering.PostProcessing
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

        private Shader _TEMP_PostProcessShader;

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
        private bool _initialized = false;

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _TEMP_PostProcessShader = new Shader(Shader.ParseShader("Core/Rendering/Shaders/Misc/PostProcessingInverted.glsl"), true);

            _renderBuffer = new FrameBuffer(4, GL_RGB, GL_COLOR_ATTACHMENT0, false, true);
            _frameBuffer1 = new FrameBuffer(0, 0, 0, true, false);
            //_frameBuffer2 = new FrameBuffer(0, 0, 0, true, false);
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

            int width = Program.Game.CurrentWindowWidth;
            int height = Program.Game.CurrentWindowHeight;
            _renderBuffer.BindRead();
            _frameBuffer1.BindWrite();
            glBlitFramebuffer(0, 0, width, height, 0, 0, width, height, GL_COLOR_BUFFER_BIT, GL_NEAREST);
            glBindFramebuffer(GL_FRAMEBUFFER, 0); // Binds both READ and WRITE framebuffer to default framebuffer
        }

        public void Render()
        {
            if (!_initialized) return;

            _TEMP_PostProcessShader.Use();
            _frameBuffer1.AttachedTexture.Use(GL_TEXTURE0);

            _vertexArray.Bind();
            glDrawArrays(GL_TRIANGLES, 0, 6);
            _vertexArray.Unbind();
        }

        public void RegisterStack(PostProcessingStack stack)
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

        public void UnregisterStack(PostProcessingStack stack)
        {
            _stacks.Remove(stack);
        }
    }
}
