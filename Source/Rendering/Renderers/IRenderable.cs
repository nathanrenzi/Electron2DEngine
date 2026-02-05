namespace Electron2D.Rendering
{
    /// <summary>
    /// An interface that allows an object to register in the render layer system.
    /// </summary>
    public interface IRenderable : IDisposable
    {
        public void Render();
        public int RenderLayer { get; }
        public bool IgnorePostProcessing { get; }
    }
}
