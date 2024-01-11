namespace Electron2D.Core.Audio
{
    public class Bank : IDisposable
    {
        private bool disposed;

        public string FileName { get; }

        private FMOD.Studio.Bank bank;

        ~Bank()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        public Bank(string _fileName, FMOD.Studio.Bank _fmodBank)
        {
            FileName = _fileName;
            bank = _fmodBank;
        }

        public FMOD.Studio.Bank GetFMODBank()
        {
            return bank;
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
