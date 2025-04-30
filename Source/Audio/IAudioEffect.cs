using NAudio.Wave;

namespace Electron2D.Audio
{
    public interface IAudioEffect : ISampleProvider
    {
        public void Initialize(ISampleProvider sampleProvider);
        //public string ToJson();
    }
}
