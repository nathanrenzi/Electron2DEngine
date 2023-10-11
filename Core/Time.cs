using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;

namespace Electron2D.Core
{
    public static class Time
    {
        public static float DeltaTime;
        public static float TotalElapsedSeconds;
    }

    public class TimeUniform : IGlobalUniform
    {
        private static TimeUniform instance = null;
        private static readonly object loc = new();
        public static TimeUniform Instance
        {
            get
            {
                lock (loc)
                {
                    if (instance is null)
                    {
                        instance = new TimeUniform();
                    }
                    return instance;
                }
            }
        }

        public void ApplyUniform(Shader _shader)
        {
            _shader.SetFloat("time", Time.TotalElapsedSeconds);
        }
    }
}
