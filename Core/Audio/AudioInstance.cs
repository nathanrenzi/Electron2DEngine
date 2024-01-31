namespace Electron2D.Core.Audio
{
    public class AudioInstance
    {
        public AudioClip AudioClip { get; }
        public AudioMode Mode { get; private set; }
        public PlaybackState PlaybackState { get; private set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }

        public AudioInstanceSampleProvider SampleProvider { get; private set; }

        public AudioInstance(AudioClip _clip, AudioMode _mode, float _volume, float _pitch)
        {
            AudioClip = _clip;
            Volume = _volume;
            Mode = _mode;
            Pitch = _pitch;

            SampleProvider = new AudioInstanceSampleProvider(this);
        }

        public void Play()
        {
            PlaybackState = PlaybackState.Playing;
            AudioSystem.PlayAudioInstance(this);
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
