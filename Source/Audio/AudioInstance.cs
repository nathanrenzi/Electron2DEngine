namespace Electron2D.Audio
{
    public class AudioInstance : IDisposable
    {
        public event Action OnFadeInEnd;
        public event Action OnFadeOutEnd;
        public AudioClip AudioClip { get; private set; }
        public AudioStream Stream { get; set; }
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
                return Stream.EnableLooping;
            }
            set
            {
                Stream.EnableLooping = value;
            }
        }

        private AudioSpatializer _spatializer;

        ~AudioInstance()
        {
            Dispose(false);
        }

        public AudioInstance(AudioClip clip, float volume, float pitch, bool isLoop, float startStopVolumeFadeTime = 0.001f)
        {
            AudioClip = clip;
            Volume = volume;
            Pitch = pitch;
            Stream = clip.GetNewStream(this, false);
            IsLoop = isLoop;
            StartStopVolumeFadeTime = startStopVolumeFadeTime;

            Stream.OnStreamEnd += Stop;
            Stream.VolumeFadeSampleProvider.OnFadeInEnd += () => OnFadeInEnd?.Invoke();
            Stream.VolumeFadeSampleProvider.OnFadeOutEnd += () => OnFadeOutEnd?.Invoke();
            Stream.SetFadeTime(StartStopVolumeFadeTime);
        }

        /// <summary>
        /// Called by <see cref="AudioSpatializer"/> to register itself in each <see cref="AudioInstance"/>.
        /// </summary>
        /// <param name="spatializer"></param>
        internal void SetSpatializer(AudioSpatializer spatializer)
        {
            _spatializer = spatializer;
            Stream.OnStreamEnd += Stop;
            Stream.VolumeFadeSampleProvider.OnFadeInEnd += () => OnFadeInEnd?.Invoke();
            Stream.VolumeFadeSampleProvider.OnFadeOutEnd += () => OnFadeOutEnd?.Invoke();
            Stream.SetFadeTime(StartStopVolumeFadeTime);
        }

        /// <summary>
        /// Sets the audio clip.
        /// </summary>
        /// <param name="clip"></param>
        public void SetAudioClip(AudioClip clip)
        {
            if(clip == null)
            {
                Debug.LogError("AudioClip is null, cannot set!");
                return;
            }
            AudioClip = clip;
            bool shouldPlay = false;
            if(PlaybackState == PlaybackState.Playing)
            {
                Stop();
                shouldPlay = true;
            }
            if(_spatializer == null)
            {
                Stream.Dispose();
                Stream = clip.GetNewStream(this, false);
                Stream.OnStreamEnd += Stop;
                Stream.VolumeFadeSampleProvider.OnFadeInEnd += () => OnFadeInEnd?.Invoke();
                Stream.VolumeFadeSampleProvider.OnFadeOutEnd += () => OnFadeOutEnd?.Invoke();
                Stream.SetFadeTime(StartStopVolumeFadeTime);
                if (shouldPlay) Play();
            }
            else
            {
                // Creates new stream using new clip and the spatializer
                AudioSpatializer spatializer = _spatializer;
                spatializer.RemoveAudioInstance(this);
                spatializer.AddAudioInstance(this);
                if (shouldPlay) Play();
            }
        }

        /// <summary>
        /// Retrieves the <see cref="AudioSpatializer"/> this <see cref="AudioInstance"/> is using.
        /// </summary>
        /// <returns>An <see cref="AudioSpatializer"/> object, or null if none is assigned.</returns>
        public AudioSpatializer GetSpatializer() => _spatializer;

        /// <summary>
        /// Adds an audio effect to this audio instance. Must be added before the audio instance is played.
        /// </summary>
        /// <param name="effect"></param>
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool freeManaged)
        {
            _spatializer?.RemoveAudioInstance(this);

            if (freeManaged)
            {
                Stream.Dispose();
            }
        }
    }

    public enum PlaybackState
    {
        Stopped,
        Paused,
        Playing
    }
}
