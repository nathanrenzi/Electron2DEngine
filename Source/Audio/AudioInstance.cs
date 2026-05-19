namespace Atlas2D.Audio
{
    public class AudioInstance : IDisposable
    {
        public event Action OnFadeInEnd;
        public event Action OnFadeOutEnd;
        public AudioStream? Stream { get; set; }
        public PlaybackState PlaybackState { get; private set; }
        public float StartStopVolumeFadeTime { get; private set; }
        public float Volume { get; set; }
        public float Panning { get; set; }
        public float Pitch { get; set; }
        public List<IAudioEffect> Effects { get; } = new List<IAudioEffect>();

        public bool IsLoop
        {
            get
            {
                if (Stream != null)
                {
                    return Stream.EnableLooping;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if(Stream != null) Stream.EnableLooping = value;
            }
        }

        private AudioSpatializer _spatializer;

        public AudioInstance(string fileName, float volume, float pitch, bool isLoop, float startStopVolumeFadeTime = 0.001f)
        {
            Volume = volume;
            Pitch = pitch;
            if(!File.Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }
            Stream = new AudioStream(this, fileName, _spatializer?.Is3D ?? false);
            IsLoop = isLoop;
            StartStopVolumeFadeTime = startStopVolumeFadeTime;
            HookStreamEvents();
        }

        public AudioInstance(AudioStream? stream, float volume, float pitch, bool isLoop, float startStopVolumeFadeTime = 0.001f)
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
            if (Stream == null) return;
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
            if (Stream != null && !Stream.WaveFormat.Equals(stream.WaveFormat))
            {
                Debug.LogError("Cannot set audio stream. WaveFormat does not match the WaveFormat of the current stream." +
                    "Check the sample rate and other settings to make sure they match.");
                return;
            }
            bool shouldPlay = PlaybackState == PlaybackState.Playing;
            if (shouldPlay) Stop();

            Stream?.Dispose();
            Stream = stream;
            Stream.EnableLooping = IsLoop;
            // re-apply effects
            HookStreamEvents();
            if (_spatializer != null)
            {
                AudioSpatializer spatializer = _spatializer;
                spatializer.RemoveAudioInstance(this);
                spatializer.AddAudioInstance(this);
            }

            if (shouldPlay) Play();
        }

        public AudioSpatializer GetSpatializer() => _spatializer;

        public void AddEffect(IAudioEffect effect)
        {
            if (Stream == null) return; 
            effect.Initialize(Stream.SampleProvider);
            Stream.SampleProvider = effect;
            Effects.Add(effect);
        }

        public void Play()
        {
            if (Stream == null) return;
            Stream.SetFadeDirection(1);
            Stream.Position = 0;
            PlaybackState = PlaybackState.Playing;
            AudioSystem.PlayAudioInstance(this);
        }

        public void Play(long position)
        {
            if (Stream == null) return;
            Stream.SetFadeDirection(1);
            Stream.Position = IsLoop ? position % Stream.Length : Math.Min(position, Stream.Length);
            PlaybackState = PlaybackState.Playing;
            AudioSystem.PlayAudioInstance(this);
        }

        public void Pause()
        {
            if (Stream == null) return;
            if (PlaybackState == PlaybackState.Stopped) return;
            Stream.SetFadeDirection(-1);
            PlaybackState = PlaybackState.Paused;
        }

        public void Unpause()
        {
            if (Stream == null) return;
            if (PlaybackState == PlaybackState.Stopped || PlaybackState == PlaybackState.Playing) return;
            Stream.SetFadeDirection(1);
            PlaybackState = PlaybackState.Playing;
            AudioSystem.PlayAudioInstance(this);
        }

        public void Stop()
        {
            if (Stream == null) return;
            Stream.SetFadeDirection(-1);
            PlaybackState = PlaybackState.Stopped;
        }

        public void Dispose()
        {
            _spatializer?.RemoveAudioInstance(this);
            Stream?.Dispose();
        }
    }

    public enum PlaybackState
    {
        Stopped,
        Paused,
        Playing
    }
}