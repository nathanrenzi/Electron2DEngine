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

        public FrameBuffer(int glRenderBufferSamples, int glRenderBufferStorageSetting, int glRenderBufferAttachmentSetting, bool attachTexture2D, bool attachRenderBuffer)
        {
            BufferID = glGenFramebuffer();
            Bind();

            if(attachTexture2D)
            {
                AttachedTexture = TextureFactory.Create(Program.Game.CurrentWindowWidth, Program.Game.CurrentWindowHeight, GL_RGB);
                glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, AttachedTexture.Handle, 0);
            }

            if(attachRenderBuffer)
            {
                RenderBufferID = glGenRenderbuffer();
                glBindRenderbuffer(RenderBufferID);
                glRenderbufferStorageMultisample(GL_RENDERBUFFER, glRenderBufferSamples, glRenderBufferStorageSetting,
                    Program.Game.CurrentWindowWidth, Program.Game.CurrentWindowHeight);
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
