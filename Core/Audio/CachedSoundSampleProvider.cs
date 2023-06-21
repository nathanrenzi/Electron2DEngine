using NAudio.Wave;

namespace Electron2D.Core.Audio
{
    public class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        private long position;

        public CachedSoundSampleProvider(CachedSound _cachedSound)
        {
            cachedSound = _cachedSound;
        }

        public int Read(float[] _buffer, int _offset, int _count)
        {
            var availableSamples = cachedSound.AudioData.Length - position;
            var samplesToCopy = Math.Min(availableSamples, _count);
            Array.Copy(cachedSound.AudioData, position, _buffer, _offset, samplesToCopy);
            position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
    }
}
