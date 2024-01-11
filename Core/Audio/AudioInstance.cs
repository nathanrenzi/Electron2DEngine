using FMOD.Studio;

namespace Electron2D.Core.Audio
{
    public class AudioInstance : IDisposable
    {
        private bool disposed;

        public bool IsPlaying
        {
            get
            {
                eventInstance.getPlaybackState(out PLAYBACK_STATE state);
                return state == PLAYBACK_STATE.PLAYING;
            }
        }
        public bool IsPaused
        {
            get
            {
                eventInstance.getPlaybackState(out PLAYBACK_STATE state);
                return state == PLAYBACK_STATE.SUSTAINING;
            }
        }
        private EventDescription eventDescription;
        private EventInstance eventInstance;

        public AudioInstance(EventDescription _eventDescription, EventInstance _eventInstance)
        {
            eventDescription = _eventDescription;
            eventInstance = _eventInstance;
        }

        ~AudioInstance()
        {
            Dispose(false);
        }

        public EventDescription GetFMODEventDescription()
        {
            return eventDescription;
        }

        public EventInstance GetFMODEventInstance()
        {
            return eventInstance;
        }

        public void Play()
        {
            eventInstance.start();
        }

        public void Pause()
        {
            eventInstance.setPaused(true);
        }

        public void Unpause()
        {
            eventInstance.setPaused(false);
        }

        public void Stop(STOP_MODE _stopMode = STOP_MODE.IMMEDIATE)
        {
            eventInstance.stop(_stopMode);
        }

        public void SetVolume(float _volume)
        {
            eventInstance.setVolume(_volume);
        }

        public float GetVolume()
        {
            eventInstance.getVolume(out float volume);
            return volume;
        }

        public void SetPitch(float _pitch)
        {
            eventInstance.setPitch(_pitch);
        }

        public float GetPitch()
        {
            eventInstance.getPitch(out float pitch);
            return pitch;
        }

        public PLAYBACK_STATE GetPlaybackState()
        {
            eventInstance.getPlaybackState(out PLAYBACK_STATE state);
            return state;
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
                eventInstance.release();
                disposed = true;
            }
        }
    }
}
