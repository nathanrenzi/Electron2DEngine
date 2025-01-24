namespace Electron2D.Audio
{
    public class AudioInstance : IDisposable
    {
        public AudioClip AudioClip { get; }
        public AudioStream Stream { get; set; }
        public PlaybackState PlaybackState { get; private set; }
        public float Volume { get; set; }
        public float VolumeMultiplier { get; set; } = 1.0f;
        public float Panning { get; set; }
        public float PanningAdditive { get; set; } = 0.0f;
        public float Pitch { get; set; }
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

        public AudioInstance(AudioClip clip, float volume, float pitch, bool isLoop)
        {
            AudioClip = clip;
            Volume = volume;
            Pitch = pitch;
            Stream = clip.GetNewStream(this, false);
            IsLoop = isLoop;

            Stream.OnStreamEnd += Stop;
        }

        /// <summary>
        /// This should only be called by AudioSpatializer to register itself in each AudioInstance.
        /// </summary>
        /// <param name="_spatializer"></param>
        public void SetSpatializerReference(AudioSpatializer spatializer)
        {
            _spatializer = spatializer;
        }

        public void Play()
        {
            Stop();
            PlaybackState = PlaybackState.Playing;
            AudioSystem.PlayAudioInstance(this);
        }

        public void Pause()
        {
            if (PlaybackState == PlaybackState.Stopped) return;
            PlaybackState = PlaybackState.Paused;
        }

        public void Unpause()
        {
            if (PlaybackState == PlaybackState.Stopped) return;
            PlaybackState = PlaybackState.Playing;
        }

        public void Stop()
        {
            Stream.Position = 0;
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
