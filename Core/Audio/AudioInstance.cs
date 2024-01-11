using FMOD.Studio;

namespace Electron2D.Core.Audio
{
    public class AudioInstance : IDisposable
    {
        private bool disposed;

        public bool IsPlaying { get; private set; }
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

        public void Stop(STOP_MODE _stopMode = STOP_MODE.IMMEDIATE)
        {
            eventInstance.stop(_stopMode);
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
