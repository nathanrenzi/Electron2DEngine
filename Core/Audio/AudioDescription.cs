using FMOD;
using FMOD.Studio;

namespace Electron2D.Core.Audio
{
    public class AudioDescription : IDisposable
    {
        private bool disposed;

        private EventDescription eventDescription;

        public AudioDescription(EventDescription _eventDescription) 
        {
            eventDescription = _eventDescription;
        }

        ~AudioDescription()
        {
            Dispose(false);
        }

        public AudioInstance CreateInstance()
        {
            RESULT result = eventDescription.createInstance(out EventInstance eventInstance);
            if (result != RESULT.OK)
            {
                eventDescription.getID(out GUID guid);
                Debug.LogError($"AUDIO: Error instancing sound with GUID: {guid}");
                return null;
            }

            return new AudioInstance(eventDescription, eventInstance);
        }

        public EventDescription GetFMODEventDescription()
        {
            return eventDescription;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool _safeToDisposeManagedObjects)
        {
            if (!disposed)
            {
                eventDescription.releaseAllInstances();
                disposed = true;
            }
        }
    }
}
