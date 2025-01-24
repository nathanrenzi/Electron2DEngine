using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Electron2D.Audio
{
    public class AudioStream : WaveStream
    {
        public Action OnStreamEnd { get; set; }
        public ISampleProvider SampleProvider { get; set; }
        public IPanStrategy PanStrategy { get; }
        public bool Is3D { get; }

        private AudioInstance audioInstance;
        private WaveStream sourceStream;
        private VolumeSampleProvider volumeSampleProvider;
        private AudioPitchSampleProvider pitchShiftingSampleProvider;
        private PanningSampleProvider panningSampleProvider;

        public AudioStream(AudioInstance _audioInstance, WaveStream _sourceStream, bool _is3D, IPanStrategy _panStrategy)
        {
            audioInstance = _audioInstance;
            sourceStream = _sourceStream;
            EnableLooping = true;
            Is3D = _is3D;

            volumeSampleProvider = new VolumeSampleProvider(this.ToSampleProvider());
            pitchShiftingSampleProvider = new AudioPitchSampleProvider(volumeSampleProvider);
            pitchShiftingSampleProvider.Pitch = audioInstance.Pitch;
            if (_panStrategy == null)
            {
                PanStrategy = new SinPanStrategy();
            }
            else
            {
                PanStrategy = _panStrategy;
            }

            if(Is3D)
            {
                //panningSampleProvider = new PanningSampleProvider(pitchShiftingSampleProvider.ToMono());
                panningSampleProvider = new PanningSampleProvider(volumeSampleProvider.ToMono());
                panningSampleProvider.PanStrategy = PanStrategy;

                SampleProvider = panningSampleProvider;
            }
            else
            {
                //SampleProvider = pitchShiftingSampleProvider;
                SampleProvider = volumeSampleProvider;
            }
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
                volumeSampleProvider.Volume = audioInstance.Volume * audioInstance.VolumeMultiplier;
                pitchShiftingSampleProvider.Pitch = audioInstance.Pitch;
                if(Is3D) panningSampleProvider.Pan = MathEx.Clamp(audioInstance.Panning + audioInstance.PanningAdditive, -1, 1);

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
                return 0;
            }
        }
    }
}
