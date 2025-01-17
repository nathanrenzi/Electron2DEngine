namespace Electron2D.Core.Rendering.PostProcessing
{
    public interface IPostProcess
    {
        /// <param name="signal">A value that will determine the behavior of the post processing pass.</param>
        /// <param name="readBuffer">The buffer that the process will read from.</param>
        /// <returns>The signal value to be passed back into the same process. Returns 0 if no further processing is needed.</returns>
        public int PostProcess(int signal, FrameBuffer readBuffer);
    }
}
