namespace Electron2D.Core.Rendering
{
    public interface IBuffer
    {
        uint bufferID { get; }
        void Bind();
        void Unbind();
        void Dispose();
    }
}
