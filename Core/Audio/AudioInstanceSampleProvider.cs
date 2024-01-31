using NAudio.Wave;

namespace Electron2D.Core.Audio
{
    public class AudioInstanceSampleProvider : ISampleProvider
    {
        private readonly AudioInstance audioInstance;
        private long position;

        public AudioInstanceSampleProvider(AudioInstance _audioInstance)
        {
            audioInstance = _audioInstance;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = audioInstance.AudioClip.AudioData.Length - position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(audioInstance.AudioClip.AudioData, position, buffer, offset, samplesToCopy);
            position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return audioInstance.AudioClip.WaveFormat; } }
    }
}
