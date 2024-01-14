namespace Electron2D.Core.Audio
{
    public class Bank
    {
        public string FileName { get; }

        private FMOD.Studio.Bank bank;

        public Bank(string _fileName, FMOD.Studio.Bank _fmodBank)
        {
            FileName = _fileName;
            bank = _fmodBank;
        }

        public FMOD.Studio.Bank GetFMODBank()
        {
            return bank;
        }
    }
}
