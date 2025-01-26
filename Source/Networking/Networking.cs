using Riptide;
using Riptide.Utils;

namespace Electron2D.Networking
{
    /// <summary>
    /// A general purpose networking class powered by <see href="https://riptide.tomweiland.net/manual/overview/get-started.html">RiptideNetworking</see>.
    /// </summary>
    public class Networking : IGameClass
    {
        public delegate NetworkGameClass CreateNetworkGameClass(string json);
        public delegate void SetNetworkGameClassRegisterID(int registerID);

        public enum NetworkingMessageType
        {
            NetworkClassCreated = 60000,
            NetworkClassUpdated = 60001,
            NetworkClassDeleted = 60002
        }

        private static Networking _instance;
        public static Networking Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new Networking();
                }
                return _instance;
            }
        }

        public Dictionary<ushort, NetworkGameClass> ClientNetworkGameClasses { get; private set; } = new();
        private Dictionary<ushort, ushort> _serverNetworkGameClassOwnership = new();
        private static List<CreateNetworkGameClass> _networkGameClassRegister = new();
        public Server Server { get; private set; }
        public long TimeServerStarted { get; private set; } = -1;
        public Client Client { get; private set; }

        public ushort ServerNetworkGameClassIDCount { get; private set; } = 0;
        public bool IsHost => IsServerRunning && Client.IsConnected;
        public bool IsServerRunning => Server.IsRunning;
        public bool IsClientConnected => Client.IsConnected;
        public bool IsClientConnecting => Client.IsConnecting;

        public event Action<string> ConnectionFailed;
        public event Action ConnectionSuccess;
        private string _serverPassword;

        public Networking()
        {
            if (_instance != null && _instance != this) return;

            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.Log, Debug.LogError, false);

            Client = new Client();
            Client.ConnectionFailed += ClientConnectionFailed;
            Client.Connected += ClientConnectionSuccess;
            Program.Game.RegisterGameClass(this);
        }

        ~Networking()
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
            if (!_networkGameClassRegister.Contains(factoryMethod))
            {
                _networkGameClassRegister.Add(factoryMethod);
                return _networkGameClassRegister.Count - 1;
            }

            return -1;
        }

        public void Update() { }

        public void FixedUpdate()
        {
            if(Server != null)
            {
                Server.Update();
            }

            Client.Update();
        }

        public void Dispose()
        {
            if(Server != null)
            {
                Server.Stop();
            }

            Client.Disconnect();

            Program.Game.UnregisterGameClass(this);
            GC.SuppressFinalize(this);
        }

        #region Server
        [MessageHandler((ushort)NetworkingMessageType.NetworkClassCreated)]
        private static void ServerReceiveNetworkClassCreation(ushort client, Message message)
        {
            // add check to see if other clients are allowed to create network classes
            int registerID = message.GetInt();
            string json = message.GetString();
            ushort networkID = Instance.GetNextNetworkID();
            Instance._serverNetworkGameClassOwnership.Add(networkID, client);
            Message returnMessage = Message.Create(MessageSendMode.Reliable,
                (ushort)NetworkingMessageType.NetworkClassCreated);
            returnMessage.AddInt(registerID);
            returnMessage.AddUShort(networkID);
            returnMessage.AddUShort(client);
            returnMessage.AddString(json);
            Instance.Server.SendToAll(returnMessage);
        }

        public ushort GetNextNetworkID()
        {
            return ServerNetworkGameClassIDCount++;
        }

        private void ServerValidateConnection(Connection pendingConnection, Message connectMessage)
        {
            string password = connectMessage.GetString();
            if(_serverPassword != "")
            {
                if(_serverPassword.Equals(password))
                {
                    Server.Accept(pendingConnection);
                }
                else
                {
                    Server.Reject(pendingConnection, Message.Create().AddString("Incorrect password."));
                }
            }

            Server.Accept(pendingConnection);
        }

        public void StartServer(ushort port, ushort maxClientCount, string password = "")
        {
            if (Server != null) return;

            _serverPassword = password;
            Server = new Server();
            Server.Start(port, maxClientCount);
            Server.HandleConnection = ServerValidateConnection;
            TimeServerStarted = DateTime.UtcNow.Ticks;
        }

        public void StopServer()
        {
            if (Server == null || !Server.IsRunning) return;

            Server.Stop();
            Server = null;
        }
        #endregion

        #region Client
        [MessageHandler((ushort)NetworkingMessageType.NetworkClassCreated)]
        private static void ClientReceiveNetworkClassCreation(Message message)
        {
            int registerID = message.GetInt();
            ushort networkID = message.GetUShort();
            ushort clientID = message.GetUShort();
            string json = message.GetString();

            if(clientID == Instance.Client.Id)
            {
                // Network game class was created by local player
                Instance.ClientNetworkGameClasses[networkID].ServerSpawn(networkID, clientID, json);
            }
            else
            {
                NetworkGameClass networkGameClass = _networkGameClassRegister[registerID](json);
                Instance.ClientNetworkGameClasses.Add(networkID, networkGameClass);
                networkGameClass.ServerSpawn(networkID, clientID, json);
            }
        }

        public bool Connect(string ip, ushort port, string password = "")
        {
            if (Client.IsConnected || Client.IsConnecting) return false;
            if (Server != null && Server.IsRunning && ip != "127.0.0.1" && ip != "localhost")
            {
                StopServer();
            }

            Message message = Message.Create();
            message.AddString(password);
            return Client.Connect($"{ip}:{port}", message: message);
        }

        private void ClientConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            ConnectionFailed?.Invoke(e.Message.GetString());
        }

        private void ClientConnectionSuccess(object? sender, EventArgs e)
        {
            ConnectionSuccess?.Invoke();
        }
        #endregion
    }
}