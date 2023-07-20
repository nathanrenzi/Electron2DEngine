namespace Electron2D.Core.Rendering
{
    /// <summary>
    /// An interface that allows an object to register in the render layer system.
    /// </summary>
    public interface IRenderable
    {
        public int GetRenderLayer();
        public void Render();
    }
}
