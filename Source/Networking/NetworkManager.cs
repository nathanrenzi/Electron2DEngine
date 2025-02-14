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

        public const ushort MIN_MESSAGE_TYPE_INTERCEPT = 60000;
        public const ushort MAX_MESSAGE_TYPE_INTERCEPT = 60004;

        private bool _initialized = false;

        /// <summary>
        /// The NetworkManager must be initialized before it can be used. See <see cref="InitializeForSteam"/> also.
        /// Network mode uses IP addresses and can use any user-specified port. Much less secure, as Steam Datagram Relay is not
        /// used to pass messages, so IP addresses are visible.
        /// </summary>
        public void InitializeForNetwork()
        {
            if (_initialized)
            {
                Debug.LogError("NetworkManager is already initialized, cannot initialize again without resetting!");
            }
            if (_instance != null && _instance != this) return;

            NetworkMode = NetworkMode.NetworkP2P;
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.Log, Debug.LogError, false);
            Client = new ClientServer.Client(NetworkMode);
            Server = new ClientServer.Server(NetworkMode);
            Client.SetServer(Server);
            Program.Game.RegisterGameClass(this);
        }

        /// <summary>
        /// The NetworkManager must be initialized before it can be used. See <see cref="InitializeForNetwork"/> also.
        /// Steam mode uses Steam Datagram Relay to send messages to the server and client, and can be faster than normal
        /// P2P in some cases. Much more secure than P2P mode as IP addresses are hidden and users must be authenticated
        /// through steam before connecting.
        /// </summary>
        public void InitializeForSteam()
        {
            if (_initialized)
            {
                Debug.LogError("NetworkManager is already initialized, cannot initialize again without resetting!");
            }
            if (_instance != null && _instance != this) return;

            NetworkMode = NetworkMode.SteamP2P;
            uint steamAppID;
            if(File.Exists("steam_appid.txt"))
            {
                steamAppID = uint.Parse(File.ReadAllText("steam_appid.txt"));
            }
            else
            {
                Debug.LogError("[Steamworks.NET] steam_appid.txt cannot be found. This is required to open the game through steam. " +
                    "If you are a developer, create a text file in the build directory and have it contain the number 480, and " +
                    "make sure this file does not get included in the release, steam auto-generates this file.");
                return;
            }

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
            catch (DllNotFoundException e)
            {
                Debug.LogError("[Steamworks.NET] Could not load steam_api.dll. It is likely missing or not in the correct location. " +
                    "Refer to README for more details.\n" + e.Message);
                Program.Game.Exit();
            }
            if (!SteamAPI.Init())
            {
                Debug.LogError("[Steamworks.NET] Steam API could not be initialized. Please make sure steam is running. If you are receiving this error while developing, please " +
                    "make sure that steam_appid.txt is in the output directory. DO NOT ship that file with the final game build, it should only be for testing purposes.");
                Program.Game.Exit();
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
            Reset();
            if(NetworkMode == NetworkMode.SteamP2P) SteamAPI.Shutdown();
            Program.Game.UnregisterGameClass(this);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Resets the NetworkManager so that it can be reinitialized;
        /// </summary>
        public void Reset()
        {
            Client.Disconnect();
            Server.Stop();
            if (NetworkMode == NetworkMode.SteamP2P)
            {
                Server.SteamServer.Shutdown();
            }
            Client = null;
            Server = null;
            _initialized = false;
        }
    }
}