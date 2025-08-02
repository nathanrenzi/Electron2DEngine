﻿namespace Electron2D.Rendering
{
    /// <summary>
    /// An interface that allows an object to register in the render layer system.
    /// </summary>
    public interface IRenderable : IDisposable
    {
        public int GetRenderLayer();
        public void Render();
        public bool ShouldIgnorePostProcessing();
    }
}
