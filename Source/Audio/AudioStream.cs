using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Electron2D.Audio
{
    public class AudioStream : WaveStream
    {
        public event Action OnStreamEnd;
        public string FileName { get; }
        public AudioVolumeFadeSampleProvider VolumeFadeSampleProvider { get; private set; }
        public ISampleProvider SampleProvider { get; set; }
        public IPanStrategy PanStrategy { get; }
        public bool Is3D { get; }
        public bool EnableLooping { get; set; }
        public override WaveFormat WaveFormat => _sourceStream.WaveFormat;
        public override long Length => _sourceStream.Length;
        public override long Position
        {
            get => _sourceStream.Position;
            set => _sourceStream.Position = value;
        }

        private AudioInstance _audioInstance;
        private WaveStream _sourceStream;
        private VolumeSampleProvider _volumeSampleProvider;
        private AudioPitchSampleProvider _pitchShiftingSampleProvider;
        private PanningSampleProvider _panningSampleProvider;

        public AudioStream(AudioInstance audioInstance, string fileName, bool is3D, IPanStrategy panStrategy = null)
        {
            FileName = fileName;
            _audioInstance = audioInstance;
            _sourceStream = new AudioFileReader(fileName);
            EnableLooping = true;
            Is3D = is3D;
            PanStrategy = panStrategy ?? new SinPanStrategy();

            _volumeSampleProvider = new VolumeSampleProvider(this.ToSampleProvider());
            _pitchShiftingSampleProvider = new AudioPitchSampleProvider(_volumeSampleProvider);

            if (Is3D)
            {
                _panningSampleProvider = new PanningSampleProvider(_volumeSampleProvider.ToMono());
                _panningSampleProvider.PanStrategy = PanStrategy;
                SampleProvider = _panningSampleProvider;
            }
            else
            {
                SampleProvider = _volumeSampleProvider;
            }

            VolumeFadeSampleProvider = new AudioVolumeFadeSampleProvider(SampleProvider);
            SampleProvider = VolumeFadeSampleProvider;
        }

        internal void SetInstance(AudioInstance instance) => _audioInstance = instance;
        public void SetFadeDirection(int direction) => VolumeFadeSampleProvider.SetFadeDirection(direction);
        public void SetFadeTime(float fade) => VolumeFadeSampleProvider.VolumeFadeTime = fade;
        public float GetFadeTime() => VolumeFadeSampleProvider.VolumeFadeTime;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (VolumeFadeSampleProvider.GetCurrentSampleCount() < 0)
            {
                _volumeSampleProvider.Volume = 0f;
                return 0;
            }

            AudioSpatializer spatializer = _audioInstance?.GetSpatializer();
            float volumeMultiplier = spatializer?.DistanceBasedVolumeMultiplier01 ?? 1f;
            float panningAdditive = spatializer?.DirectionBasedPanning ?? 0f;

            _volumeSampleProvider.Volume = (_audioInstance?.Volume ?? 1f) * volumeMultiplier;
            _pitchShiftingSampleProvider.Pitch = _audioInstance?.Pitch ?? 1f;
            if (Is3D) _panningSampleProvider.Pan = MathEx.Clamp((_audioInstance?.Panning ?? 0f) + panningAdditive, -1, 1);

            int totalBytesRead = 0;
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
    }
}