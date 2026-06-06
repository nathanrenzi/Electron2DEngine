using NAudio.Wave;

namespace Atlas2D.Audio
{
    public class StereoPanningSampleProvider : ISampleProvider
    {
        public WaveFormat WaveFormat { get; }
        public float Pan { get; set; } // -1 to 1

        private readonly ISampleProvider _source;

        public StereoPanningSampleProvider(ISampleProvider source)
        {
            if (source.WaveFormat.Channels != 2)
                throw new ArgumentException("Source must be stereo.");
            _source = source;
            WaveFormat = source.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = _source.Read(buffer, offset, count);
            float leftScale = Pan <= 0 ? 1f : 1f - Pan;
            float rightScale = Pan >= 0 ? 1f : 1f + Pan;
            for (int i = offset; i < offset + read; i += 2)
            {
                buffer[i] *= leftScale;
                buffer[i + 1] *= rightScale;
            }
            return read;
        }
    }
}
