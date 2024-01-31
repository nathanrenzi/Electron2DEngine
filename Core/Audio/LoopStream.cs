using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Electron2D.Core.Audio
{
    public class LoopStream : WaveStream
    {
        public Action OnStreamEnd { get; set; }
        public ISampleProvider SampleProvider { get; set; }

        private AudioInstance audioInstance;
        private WaveStream sourceStream;
        private VolumeSampleProvider volumeSampleProvider;

        public LoopStream(AudioInstance _audioInstance, WaveStream _sourceStream)
        {
            audioInstance = _audioInstance;
            sourceStream = _sourceStream;
            EnableLooping = true;

            volumeSampleProvider = new VolumeSampleProvider(this.ToSampleProvider());
            SampleProvider = volumeSampleProvider;
        }

        public bool EnableLooping { get; set; }

        public override WaveFormat WaveFormat => sourceStream.WaveFormat;

        public override long Length => sourceStream.Length;

        public override long Position
        {
            get { return sourceStream.Position; }
            set { sourceStream.Position = value; }
        }

        public override int Read(byte[] _buffer, int _offset, int _count)
        {
            int totalBytesRead = 0;

            if(audioInstance.PlaybackState == PlaybackState.Playing)
            {
                volumeSampleProvider.Volume = 1.0f;
                while (totalBytesRead < _count)
                {
                    int bytesRead = sourceStream.Read(_buffer, _offset + totalBytesRead, _count - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        if (sourceStream.Position == 0 || !EnableLooping)
                        {
                            // something wrong with the source stream
                            OnStreamEnd?.Invoke();
                            break;
                        }
                        // loop
                        sourceStream.Position = 0;
                    }
                    totalBytesRead += bytesRead;
                }
                return totalBytesRead;
            }
            else
            {
                volumeSampleProvider.Volume = 0.0f;
                return _count;
            }
        }
    }
}
