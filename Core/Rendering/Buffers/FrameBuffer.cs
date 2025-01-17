using Electron2D.Core.Management;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public class FrameBuffer : IBuffer, IDisposable
    {
        public uint BufferID { get; }
        public uint RenderBufferID { get; private set; } = 99999999;
        public Texture2D AttachedTexture { get; private set; }

        private bool _isDisposed = false;

        public FrameBuffer(int glRenderBufferSamples, int glRenderBufferFormatSetting, int glRenderBufferAttachmentSetting, bool attachTexture2D, bool attachRenderBuffer)
        {
            BufferID = glGenFramebuffer();
            Bind();

            if(attachTexture2D)
            {
                AttachedTexture = TextureFactory.Create((int)Display.WindowSize.X, (int)Display.WindowSize.Y,
                    GL_RGB, GL_NEAREST, GL_CLAMP_TO_EDGE);
                AttachedTexture.Use(GL_TEXTURE0);
                glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, AttachedTexture.Handle, 0);
            }

            if(attachRenderBuffer)
            {
                RenderBufferID = glGenRenderbuffer();
                glBindRenderbuffer(RenderBufferID);
                // Handling multisampled vs nonmultisampled renderbuffers
                if(glRenderBufferSamples <= 1)
                {
                    glRenderbufferStorage(GL_RENDERBUFFER, glRenderBufferFormatSetting, (int)Display.WindowSize.X,
                        (int)Display.WindowSize.Y);
                }
                else
                {
                    glRenderbufferStorageMultisample(GL_RENDERBUFFER, glRenderBufferSamples, glRenderBufferFormatSetting,
                        (int)Display.WindowSize.X, (int)Display.WindowSize.Y);
                }
                glFramebufferRenderbuffer(GL_FRAMEBUFFER, glRenderBufferAttachmentSetting, GL_RENDERBUFFER, RenderBufferID);
                glBindRenderbuffer(0);
            }

            if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
            {
                Debug.LogError($"FrameBuffer object was not successfully created. FrameBuffer status: [{glCheckFramebufferStatus(GL_FRAMEBUFFER)}]");
            }

            Unbind();
        }

        ~FrameBuffer()
        {
            Dispose();
        }

        public void Save(string _filePath)
        {
            throw new NotImplementedException();

            BindRead();
            glReadBuffer(GL_COLOR_ATTACHMENT0);
        }

        public void Bind()
        {
            glBindFramebuffer(GL_FRAMEBUFFER, BufferID);
        }

        public void BindRead()
        {
            glBindFramebuffer(GL_READ_FRAMEBUFFER, BufferID);
        }

        public void BindWrite()
        {
            glBindFramebuffer(GL_DRAW_FRAMEBUFFER, BufferID);
        }

        public void UnbindRead()
        {
            glBindFramebuffer(GL_READ_FRAMEBUFFER, 0);
        }

        public void UnbindWrite()
        {
            glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0);
        }

        public void Unbind()
        {
            glBindFramebuffer(GL_FRAMEBUFFER, 0);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                if(AttachedTexture != null)
                {
                    AttachedTexture.Dispose();
                }
                if(RenderBufferID != 99999999)
                {
                    glDeleteRenderbuffer(RenderBufferID);
                }
                glDeleteFramebuffer(BufferID);
            }
        }
    }
}
