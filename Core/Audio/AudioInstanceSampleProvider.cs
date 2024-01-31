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

        public int Read(float[] _buffer, int _offset, int _count)
        {
            if(audioInstance.PlaybackState == PlaybackState.Playing)
            {
                var availableSamples = audioInstance.AudioClip.AudioData.Length - position;
                var samplesToCopy = Math.Min(availableSamples, _count);
                Array.Copy(audioInstance.AudioClip.AudioData, position, _buffer, _offset, samplesToCopy);
                position += samplesToCopy;
                return (int)samplesToCopy;
            }
            else
            {
                return _count;
            }
        }

        public WaveFormat WaveFormat { get { return audioInstance.AudioClip.WaveFormat; } }
    }
}
