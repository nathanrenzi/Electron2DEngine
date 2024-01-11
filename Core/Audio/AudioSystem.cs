using FMOD;
using FMOD.Studio;

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

        #region Banks
        /// <summary>
        /// Loads an FMOD bank file.
        /// </summary>
        /// <param name="_fileName"></param>
        /// <param name="_loadSampleData">Whether or not the data inside of the bank should be loaded into memory once the bank is created.</param>
        /// <returns></returns>
        public static Bank LoadBank(string _fileName, bool _loadSampleData = false)
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

                Debug.Log($"Loaded [{_fileName}] into memory.", ConsoleColor.Yellow);

                if(_loadSampleData)
                {
                    RESULT result2 = bank.GetFMODBank().loadSampleData();
                    if(result2 != RESULT.OK)
                    {
                        Debug.LogError($"AUDIO: Could not load sample data on bank {_fileName}. The bank object will still be created.");
                    }
                }

                return bank;
            }
        }

        /// <summary>
        /// Returns all currently loaded banks.
        /// </summary>
        /// <returns></returns>
        public static Bank[] GetLoadedBanks()
        {
            List<Bank> returnList = new List<Bank>();

            foreach (var item in loadedBanks)
            {
                returnList.Add(item.Value);
            }

            return returnList.ToArray();
        }

        /// <summary>
        /// Unloads all currently loaded banks.
        /// </summary>
        public static void UnloadAllBanks()
        {
            audioSystem.unloadAll();
            loadedBanks.Clear();
        }
        #endregion

        #region Audio Instances
        public static AudioInstance CreateInstance(string _guid)
        {
            RESULT result1 = GetFMODSystem().getEvent(_guid, out EventDescription eventDescription);
            if(result1 != RESULT.OK)
            {
                Debug.LogError($"AUDIO: Error getting sound with GUID: {_guid}");
                return null;
            }

            RESULT result2 = eventDescription.createInstance(out EventInstance eventInstance);
            if (result2 != RESULT.OK)
            {
                Debug.LogError($"AUDIO: Error instancing sound with GUID: {_guid}");
                return null;
            }

            return new AudioInstance(eventDescription, eventInstance);
        }

        public static AudioDescription CreateDescription(string _guid)
        {
            RESULT result = GetFMODSystem().getEvent(_guid, out EventDescription eventDescription);
            if (result != RESULT.OK)
            {
                Debug.LogError($"AUDIO: Error getting sound for GUID: {_guid}");
                return null;
            }
            eventDescription.loadSampleData();

            return new AudioDescription(eventDescription);
        }
        #endregion

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
