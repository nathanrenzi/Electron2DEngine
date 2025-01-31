using Riptide;
using Riptide.Utils;
using Electron2D.Networking.ClientServer;

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

        public const ushort MIN_NETWORK_MESSAGE_TYPE = 60000;
        public const ushort MAX_NETWORK_MESSAGE_TYPE = 60004;

        /// <summary>
        /// This must be called before the NetworkManager can be used.
        /// </summary>
        public void Initialize()
        {
            if (_instance != null && _instance != this) return;

            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.Log, Debug.LogError, false);

            Client = new ClientServer.Client();
            Server = new ClientServer.Server();
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

            Program.Game.UnregisterGameClass(this);
            GC.SuppressFinalize(this);
        }
    }
}