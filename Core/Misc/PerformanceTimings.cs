using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electron2D.Core.Misc
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
