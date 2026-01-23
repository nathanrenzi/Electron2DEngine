using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Electron2D.Audio
{
    public class AudioStream : WaveStream
    {
        public event Action OnStreamEnd;
        public AudioVolumeFadeSampleProvider VolumeFadeSampleProvider { get; private set; }
        public ISampleProvider SampleProvider { get; set; }
        public IPanStrategy PanStrategy { get; }
        public bool Is3D { get; }
        public bool EnableLooping { get; set; }
        public override WaveFormat WaveFormat => _sourceStream.WaveFormat;
        public override long Length => _sourceStream.Length;
        public override long Position
        {
            get { return _sourceStream.Position; }
            set { _sourceStream.Position = value; }
        }

        private AudioInstance _audioInstance;
        private WaveStream _sourceStream;
        private VolumeSampleProvider _volumeSampleProvider;
        private AudioPitchSampleProvider _pitchShiftingSampleProvider;
        private PanningSampleProvider _panningSampleProvider;

        public AudioStream(AudioInstance audioInstance, WaveStream sourceStream, bool is3D, IPanStrategy panStrategy)
        {
            _audioInstance = audioInstance;
            _sourceStream = sourceStream;
            EnableLooping = true;
            Is3D = is3D;

            _volumeSampleProvider = new VolumeSampleProvider(this.ToSampleProvider());
            _pitchShiftingSampleProvider = new AudioPitchSampleProvider(_volumeSampleProvider);
            _pitchShiftingSampleProvider.Pitch = _audioInstance.Pitch;
            if (panStrategy == null)
            {
                PanStrategy = new SinPanStrategy();
            }
            else
            {
                PanStrategy = panStrategy;
            }

            if (Is3D)
            {
                //panningSampleProvider = new PanningSampleProvider(pitchShiftingSampleProvider.ToMono());
                _panningSampleProvider = new PanningSampleProvider(_volumeSampleProvider.ToMono());
                _panningSampleProvider.PanStrategy = PanStrategy;

                SampleProvider = _panningSampleProvider;
            }
            else
            {
                //SampleProvider = pitchShiftingSampleProvider;
                SampleProvider = _volumeSampleProvider;
            }

            VolumeFadeSampleProvider = new AudioVolumeFadeSampleProvider(SampleProvider);
            SampleProvider = VolumeFadeSampleProvider;
        }

        public void SetFadeDirection(int direction) => VolumeFadeSampleProvider.SetFadeDirection(direction);
        public void SetFadeTime(float fade) => VolumeFadeSampleProvider.VolumeFadeTime = fade;
        public float GetFadeTime() => VolumeFadeSampleProvider.VolumeFadeTime;

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            if(VolumeFadeSampleProvider.GetCurrentSampleCount() >= 0)
            {
                AudioSpatializer spatializer = _audioInstance.GetSpatializer();
                float volumeMultiplier = 1;
                float panningAdditive = 0;
                if(spatializer != null)
                {
                    volumeMultiplier = spatializer.DistanceBasedVolumeMultiplier01;
                    panningAdditive = spatializer.DirectionBasedPanning;
                }
                _volumeSampleProvider.Volume = _audioInstance.Volume * volumeMultiplier;
                _pitchShiftingSampleProvider.Pitch = _audioInstance.Pitch;
                if(Is3D) _panningSampleProvider.Pan = MathEx.Clamp(_audioInstance.Panning + panningAdditive, -1, 1);

                while (totalBytesRead < count)
                {
                    int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        if (_sourceStream.Position == 0 || !EnableLooping)
                        {
                            OnStreamEnd?.Invoke();
                            break;
                        }

                        _sourceStream.Position = 0;
                        continue;
                    }
                    totalBytesRead += bytesRead;
                }

                return totalBytesRead;
            }
            else
            {
                _volumeSampleProvider.Volume = 0.0f;
                return 0;
            }
        }
    }
}
