namespace Electron2D.Audio
{
    public class AudioInstance : IDisposable
    {
        public event Action OnFadeInEnd;
        public event Action OnFadeOutEnd;
        public AudioStream Stream { get; set; }
        public PlaybackState PlaybackState { get; private set; }
        public float StartStopVolumeFadeTime { get; private set; }
        public float Volume { get; set; }
        public float Panning { get; set; }
        public float Pitch { get; set; }
        public List<IAudioEffect> Effects { get; } = new List<IAudioEffect>();

        public bool IsLoop
        {
            get => Stream.EnableLooping;
            set => Stream.EnableLooping = value;
        }

        private AudioSpatializer _spatializer;

        public AudioInstance(AudioStream stream, float volume, float pitch, bool isLoop, float startStopVolumeFadeTime = 0.001f)
        {
            Volume = volume;
            Pitch = pitch;
            Stream = stream;
            IsLoop = isLoop;
            StartStopVolumeFadeTime = startStopVolumeFadeTime;
            HookStreamEvents();
        }

        internal void SetSpatializer(AudioSpatializer spatializer)
        {
            _spatializer = spatializer;
        }

        private void HookStreamEvents()
        {
            Stream.OnStreamEnd += Stop;
            Stream.VolumeFadeSampleProvider.OnFadeInEnd += () => OnFadeInEnd?.Invoke();
            Stream.VolumeFadeSampleProvider.OnFadeOutEnd += () => OnFadeOutEnd?.Invoke();
            Stream.SetFadeTime(StartStopVolumeFadeTime);
        }

        public void SetAudioStream(AudioStream stream)
        {
            if (stream == null)
            {
                Debug.LogError("AudioStream is null, cannot set!");
                return;
            }

            bool shouldPlay = PlaybackState == PlaybackState.Playing;
            if (shouldPlay) Stop();

            if (_spatializer == null)
            {
                Stream.Dispose();
                Stream = stream;
                HookStreamEvents();
            }
            else
            {
                Stream.Dispose();
                Stream = stream;
                AudioSpatializer spatializer = _spatializer;
                spatializer.RemoveAudioInstance(this);
                spatializer.AddAudioInstance(this);
            }

            if (shouldPlay) Play();
        }

        public AudioSpatializer GetSpatializer() => _spatializer;

        public void AddEffect(IAudioEffect effect)
        {
            effect.Initialize(Stream.SampleProvider);
            Stream.SampleProvider = effect;
            Effects.Add(effect);
        }

        public void Play()
        {
            Stream.SetFadeDirection(1);
            Stream.Position = 0;
            PlaybackState = PlaybackState.Playing;
            AudioSystem.PlayAudioInstance(this);
        }

        public void Play(long position)
        {
            Stream.SetFadeDirection(1);
            Stream.Position = IsLoop ? position % Stream.Length : Math.Min(position, Stream.Length);
            PlaybackState = PlaybackState.Playing;
            AudioSystem.PlayAudioInstance(this);
        }

        public void Pause()
        {
            if (PlaybackState == PlaybackState.Stopped) return;
            Stream.SetFadeDirection(-1);
            PlaybackState = PlaybackState.Paused;
        }

        public void Unpause()
        {
            if (PlaybackState == PlaybackState.Stopped || PlaybackState == PlaybackState.Playing) return;
            Stream.SetFadeDirection(1);
            PlaybackState = PlaybackState.Playing;
            AudioSystem.PlayAudioInstance(this);
        }

        public void Stop()
        {
            Stream.SetFadeDirection(-1);
            PlaybackState = PlaybackState.Stopped;
        }

        public void Dispose()
        {
            _spatializer?.RemoveAudioInstance(this);
            Stream.Dispose();
        }
    }

    public enum PlaybackState
    {
        Stopped,
        Paused,
        Playing
    }
}