namespace Electron2D.Misc
{
    public static class PerformanceTimings
    {
        /// <summary>
        /// Includes post processing milliseconds.
        /// </summary>
        public static double RenderMilliseconds;
        public static double PostProcessingMilliseconds;
        public static double PhysicsMilliseconds;
        public static double GameObjectMilliseconds;
        public static double FramesPerSecond;
    }
}
