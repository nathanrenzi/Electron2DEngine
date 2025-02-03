using Riptide;
using Riptide.Utils;
using Electron2D.Networking.ClientServer;
using Steamworks;

namespace Electron2D.Networking
{
    /// <summary>
    /// A general purpose networking class powered by <see href="https://riptide.tomweiland.net/manual/overview/get-started.html">RiptideNetworking</see>.
    /// </summary>
    public class NetworkManager : IGameClass
    {
        public delegate NetworkGameClass CreateNetworkGameClass(string json);
        public delegate void SetNetworkGameClassRegisterID(int registerID);
        public static List<CreateNetworkGameClass> NetworkGameClassRegister { get; private set; } = new();

        public static NetworkManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new NetworkManager();
                }
                return _instance;
            }
        }
        private static NetworkManager _instance;

        public ClientServer.Server Server { get; private set; }
        public ClientServer.Client Client { get; private set; }

        public NetworkMode NetworkMode { get; private set; }

        public const ushort MIN_NETWORK_MESSAGE_TYPE = 60000;
        public const ushort MAX_NETWORK_MESSAGE_TYPE = 60004;

        /// <summary>
        /// This must be called before the NetworkManager can be used.
        /// </summary>
        public void Initialize()
        {
            Initialize(0);
        }
        public void Initialize(uint steamAppID)
        {
            if (_instance != null && _instance != this) return;

            NetworkMode = steamAppID == 0 ? NetworkMode.NetworkP2P : NetworkMode.SteamP2P;
            if(NetworkMode == NetworkMode.SteamP2P)
            {
                Debug.Log("\n");
                if (!Packsize.Test())
                {
                    Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
                }
                if (!DllCheck.Test())
                {
                    Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
                }
                try
                {
                    if (SteamAPI.RestartAppIfNecessary((AppId_t)steamAppID))
                    {
                        Debug.Log("[Steamworks.NET] Game was not started through steam. Restarting...");
                        Program.Game.Exit();
                    }
                }
                catch(DllNotFoundException e)
                {
                    Debug.LogError("[Steamworks.NET] Could not load steam_api.dll. It is likely missing or not in the correct location. " +
                        "Refer to README for more details.\n" + e.Message);
                    Program.Game.Exit();
                }
                if(!SteamAPI.Init())
                {
                    Debug.LogError("[Steamworks.NET] Steam API could not be initialized. Please make sure steam is running. If you are receiving this error while developing, please " +
                        "make sure that steam_appid.txt is in the output directory. DO NOT ship that file with the final game build, it should only be for testing purposes.");
                    Program.Game.Exit();
                }
            }
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.Log, Debug.LogError, false);

            Client = new ClientServer.Client(NetworkMode);
            Server = new ClientServer.Server(NetworkMode);
            Client.SetServer(Server);
            Program.Game.RegisterGameClass(this);
        }

        ~NetworkManager()
        {
            Dispose();
        }

        /// <summary>
        /// Registers the factory method of a NetworkGameClass subclass so that they can be created at runtime.
        /// </summary>
        /// <param name="factoryMethod">A static method that creates the NetworkGameClass.</param>
        /// <returns>The register ID of the class. This must be assigned to a register value in each subclass.</returns>
        public static int RegisterNetworkGameClass(CreateNetworkGameClass factoryMethod)
        {
            if (!NetworkGameClassRegister.Contains(factoryMethod))
            {
                NetworkGameClassRegister.Add(factoryMethod);
                return NetworkGameClassRegister.Count - 1;
            }

            return -1;
        }

        public void Update()
        {
            if(NetworkMode == NetworkMode.SteamP2P) SteamAPI.RunCallbacks();
            Client.ClientUpdate();
        }

        public void FixedUpdate()
        {
            if(Server != null)
            {
                Server.ServerFixedUpdate();
            }

            Client.ClientFixedUpdate();
        }

        public void Dispose()
        {
            Client.Disconnect();
            Server.Stop();
            if(NetworkMode == NetworkMode.SteamP2P)
            {
                SteamAPI.Shutdown();
            }

            Program.Game.UnregisterGameClass(this);
            GC.SuppressFinalize(this);
        }
    }
}