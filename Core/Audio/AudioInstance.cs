namespace Electron2D.Core.Audio
{
    public class AudioInstance : IDisposable
    {
        private bool disposed;

        public string FileName { get; }
        public AudioMode Mode { get; private set; }
        public PlaybackState PlaybackState { get; private set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }

        protected AudioInstance(string _fileName, AudioMode _mode, float _volume, float _pitch)
        {
            FileName = _fileName;
            Volume = _volume;
            Mode = _mode;
            Pitch = _pitch;
        }

        ~AudioInstance()
        {
            Dispose(false);
        }

        public void Play()
        {
            PlaybackState = PlaybackState.Playing;
        }

        public void Pause()
        {
            PlaybackState = PlaybackState.Paused;
        }

        public void Unpause()
        {
            PlaybackState = PlaybackState.Playing;
        }

        public void Stop()
        {
            PlaybackState = PlaybackState.Stopped;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool _safeToDisposeManagedObjects)
        {
            if(!disposed)
            {
                //release
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

    public enum AudioMode
    {
        Stereo,
        Spatial
    }
}
