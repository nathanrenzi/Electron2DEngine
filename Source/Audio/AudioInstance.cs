namespace Electron2D.Audio
{
    public class AudioInstance : IDisposable
    {
        private bool disposed;

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

        private AudioSpatializer spatializer;

        ~AudioInstance()
        {
            Disposep();
        }

        public AudioInstance(AudioClip _clip, float _volume, float _pitch, bool _isLoop)
        {
            AudioClip = _clip;
            Volume = _volume;
            Pitch = _pitch;
            Stream = _clip.GetNewStream(this, false);
            IsLoop = _isLoop;

            Stream.OnStreamEnd += Stop;
        }

        /// <summary>
        /// This should only be called by AudioSpatializer to register itself in each AudioInstance.
        /// </summary>
        /// <param name="_spatializer"></param>
        public void SetSpatializerReference(AudioSpatializer _spatializer)
        {
            spatializer = _spatializer;
        }

        public void Play()
        {
            if (PlaybackState == PlaybackState.Playing) return;
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
            if (PlaybackState == PlaybackState.Stopped) return;
            PlaybackState = PlaybackState.Stopped;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Disposep();
        }

        private void Disposep()
        {
            if(!disposed)
            {
                spatializer?.RemoveAudioInstance(this);
                Stream.Dispose();

                disposed = true;
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
