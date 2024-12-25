namespace Electron2D.Core.Rendering
{
    public interface IBuffer
    {
        uint BufferID { get; }
        void Bind();
        void Unbind();
        void Dispose();
    }
}
