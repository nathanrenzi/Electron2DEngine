using FMOD;

namespace Electron2D.Core.Audio
{
    public static class AudioSystem
    {
        private static FMOD.Studio.System audioSystem;
        private static Dictionary<string, Bank> loadedBanks = new Dictionary<string, Bank>();

        public static void Initialize(int _maxChannels, FMOD.Studio.INITFLAGS _studioInitFlags, FMOD.INITFLAGS _coreInitFlags, IntPtr _extraDriverData)
        {
            Debug.Log("Starting FMOD...");
            FMOD.System s;
            RESULT testResult = Factory.System_Create(out s);
            if (testResult != RESULT.OK)
            {
                Debug.LogError($"AUDIO: Could not load FMOD core. Code: {testResult}");
                return;
            }
            s.release();

            RESULT result = FMOD.Studio.System.create(out audioSystem);
            if(result != RESULT.OK)
            {
                Debug.LogError($"AUDIO: Could not load FMOD. Code: {result}");
                return;
            }
            audioSystem.initialize(_maxChannels, _studioInitFlags, _coreInitFlags, _extraDriverData);

            Debug.Log("Finished starting FMOD");
        }

        public static Bank LoadBank(string _fileName)
        {
            if(loadedBanks.ContainsKey(_fileName))
            {
                return loadedBanks[_fileName];
            }
            else
            {
                FMOD.Studio.Bank data;
                RESULT result = audioSystem.loadBankFile(_fileName, FMOD.Studio.LOAD_BANK_FLAGS.NORMAL, out data);
                if(result != RESULT.OK)
                {
                    Debug.LogError($"AUDIO: Could not load FMOD bank {_fileName}. Code: {result}");
                    return null;
                }

                Bank bank = new Bank(_fileName, data);
                loadedBanks.Add(_fileName, bank);
                
                return bank;
            }
        }

        public static Bank[] GetLoadedBanks()
        {
            List<Bank> returnList = new List<Bank>();

            foreach (var item in loadedBanks)
            {
                returnList.Add(item.Value);
            }

            return returnList.ToArray();
        }

        public static void UnloadAllBanks()
        {
            audioSystem.unloadAll();
        }

        public static FMOD.Studio.System GetFMODSystem()
        {
            return audioSystem;
        }

        public static void Update()
        {
            audioSystem.update();
        }

        public static void Dispose()
        {
            audioSystem.release();
        }
    }
}
