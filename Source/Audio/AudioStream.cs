using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Atlas2D.Audio
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

        private WaveStream _sourceStream;
        private VolumeSampleProvider _volumeSampleProvider;
        private PanningSampleProvider _panningSampleProvider;

        private float _volume = 1f;
        private float _panning = 0f;
        private float _spatialVolumeMultiplier = 1f;
        private float _spatialPanningAdditive = 0f;
        private volatile bool _streamEnded;

        public AudioStream(string fileName, bool is3D, IPanStrategy panStrategy = null)
        {
            FileName = fileName;
            _sourceStream = new AudioFileReader(fileName);
            EnableLooping = true;
            Is3D = is3D;
            PanStrategy = panStrategy ?? new SinPanStrategy();

            _volumeSampleProvider = new VolumeSampleProvider(this.ToSampleProvider());

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

        public void SetFadeDirection(int direction)
        {
            if (direction > 0) _streamEnded = false;
            VolumeFadeSampleProvider.SetFadeDirection(direction);
        }

        public void SetFadeTime(float fade) => VolumeFadeSampleProvider.VolumeFadeTime = fade;
        public float GetFadeTime() => VolumeFadeSampleProvider.VolumeFadeTime;

        /// <summary>
        /// Sets the base volume and panning values read during playback.
        /// </summary>
        public void SetAudioValues(float volume, float panning)
        {
            _volume = volume;
            _panning = panning;
        }

        /// <summary>
        /// Sets the spatialization multipliers applied on top of base audio values during playback.
        /// </summary>
        public void SetSpatializationValues(float volumeMultiplier, float panningAdditive)
        {
            _spatialVolumeMultiplier = volumeMultiplier;
            _spatialPanningAdditive = panningAdditive;
        }

        internal void FireStreamEnd() => OnStreamEnd?.Invoke();

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (VolumeFadeSampleProvider.GetCurrentSampleCount() < 0)
            {
                _volumeSampleProvider.Volume = 0f;
                return 0;
            }

            _volumeSampleProvider.Volume = _volume * _spatialVolumeMultiplier;
            if (Is3D) _panningSampleProvider.Pan = MathEx.Clamp(_panning + _spatialPanningAdditive, -1, 1);

            int totalBytesRead = 0;
            while (totalBytesRead < count)
            {
                int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (_sourceStream.Position == 0 || !EnableLooping)
                    {
                        if (!_streamEnded)
                        {
                            _streamEnded = true;
                            AudioSystem.NotifyStreamEnded(this);
                        }
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
