namespace Electron2D.Rendering
{
    public interface IBuffer
    {
        uint BufferID { get; }
        void Bind();
        void Unbind();
        void Dispose();
    }
}
