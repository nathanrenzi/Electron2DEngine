namespace Electron2D.Core.Rendering.PostProcessing
{
    public interface IPostProcess
    {
        public void PostProcess(FrameBuffer readBuffer, FrameBuffer writeBuffer);
    }
}
