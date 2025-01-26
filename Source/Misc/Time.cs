using Electron2D.Rendering;
using Electron2D.Rendering.Shaders;

namespace Electron2D
{
    public static class Time
    {
        public static float DeltaTime;
        public static float FixedDeltaTime;
        public static float GameTime;
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

        public bool IsDirty { get; set; } = true;

        public void CheckDirty() { } // Does not need to be implemented as time will always need to be set

        public void ApplyUniform(Shader _shader)
        {
            _shader.SetFloat("time", Time.GameTime);
        }
    }
}
