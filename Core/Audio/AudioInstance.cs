using NAudio.Wave;

namespace Electron2D.Core.Audio
{
    public class AudioInstance : IDisposable
    {
        private bool disposed;

        public AudioClip AudioClip { get; }
        public LoopStream Stream { get; }
        public AudioMode Mode { get; private set; }
        public PlaybackState PlaybackState { get; private set; }
        public float Volume { get; set; }
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

        ~AudioInstance()
        {
            GC.SuppressFinalize(this);
            Dispose();
        }

        public AudioInstance(AudioClip _clip, AudioMode _mode, float _volume, float _pitch, bool _isLoop)
        {
            AudioClip = _clip;
            Volume = _volume;
            Mode = _mode;
            Pitch = _pitch;
            Stream = _clip.GetStream(this);
            IsLoop = _isLoop;

            Stream.OnStreamEnd += Stop;
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
            AudioSystem.RemoveAudioInstanceInput(this);
        }

        public void Dispose()
        {
            if(!disposed)
            {
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

    public enum AudioMode
    {
        Audio_2D,
        Audio_3D
    }
}
