using NAudio.Wave;

namespace Electron2D.Audio
{
    public class AudioInstance : IDisposable
    {
        public AudioClip AudioClip { get; }
        public AudioStream Stream { get; set; }
        public PlaybackState PlaybackState { get; private set; }
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

            Stream.OnStreamEnd += Stop;
            Stream.SetFadeTime(startStopVolumeFadeTime);
        }

        /// <summary>
        /// This should only be called by <see cref="AudioSpatializer"/> to register itself in each <see cref="AudioInstance"/>.
        /// </summary>
        /// <param name="spatializer"></param>
        public void SetSpatializer(AudioSpatializer spatializer)
        {
            _spatializer = spatializer;
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
