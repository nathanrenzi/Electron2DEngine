namespace Electron2D.Core.Audio
{
    public class Bank : IDisposable
    {
        private bool disposed;

        public string FileName { get; }
        public FMOD.Studio.Bank Data { get; }

        ~Bank()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        public Bank(string _fileName, FMOD.Studio.Bank _data)
        {
            FileName = _fileName;
            Data = _data;
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
                if(_safeToDisposeManagedObjects)
                {

                }
                disposed = true;
            }
        }
    }
}
