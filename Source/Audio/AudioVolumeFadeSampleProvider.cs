using NAudio.Wave;

namespace Electron2D.Audio
{
    public class AudioVolumeFadeSampleProvider : ISampleProvider
    {
        public float VolumeFadeTime { get; set; } = 0;
        public ISampleProvider Source { get; private set; }
        public WaveFormat WaveFormat => Source.WaveFormat;

        private int _currentSampleCount = 1;
        private int _maxSampleCount = 1;
        private int _direction;

        public void SetFadeDirection(int direction)
        {
            _direction = Math.Sign(direction);
            _maxSampleCount = (int)(VolumeFadeTime * WaveFormat.SampleRate);
            _currentSampleCount = (int)MathEx.Clamp(_currentSampleCount, 0, _maxSampleCount);
        }

        public int GetCurrentSampleCount() => _currentSampleCount;

        public AudioVolumeFadeSampleProvider(ISampleProvider source)
        {
            Source = source;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = Source.Read(buffer, offset, count);

            if(_maxSampleCount > 0)
            {
                int channels = WaveFormat.Channels;
                int totalFrames = samplesRead / channels;

                for (int i = 0; i < totalFrames; i++)
                {
                    float fadeMultiplier = MathEx.Clamp01((float)_currentSampleCount / _maxSampleCount);
                    for (int ch = 0; ch < channels; ch++)
                    {
                        int index = offset + i * channels + ch;
                        buffer[index] *= fadeMultiplier;
                    }
                    _currentSampleCount += _direction;
                }
            }

            return samplesRead;
        }
    }
}
