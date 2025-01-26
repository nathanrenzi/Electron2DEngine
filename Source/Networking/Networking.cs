using Electron2D.Rendering;
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
            NetworkClassDeleted = 60002,
            NetworkClassSync = 60003 // TODO
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

        public Dictionary<string, NetworkGameClass> ClientNetworkGameClasses { get; private set; } = new();
        private Dictionary<string, ushort> _serverNetworkGameClassOwnership = new();
        private static List<CreateNetworkGameClass> _networkGameClassRegister = new();
        public Server Server { get; private set; }
        public long TimeServerStarted { get; private set; } = -1;
        public Client Client { get; private set; }

        public string LastUsedNetworkID { get; set; } = string.Empty;
        public bool IsHost => Server != null && IsServerRunning && Client.IsConnected;
        public bool IsServerRunning => Server.IsRunning;
        public bool IsClientConnected => Client.IsConnected;
        public bool IsClientConnecting => Client.IsConnecting;

        public event Action NetworkGameClassesLoaded;
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
            Disconnect();

            Program.Game.UnregisterGameClass(this);
            GC.SuppressFinalize(this);
        }

        #region Server
        [MessageHandler((ushort)NetworkingMessageType.NetworkClassCreated)]
        private static void ServerReceiveNetworkClassCreation(ushort client, Message message)
        {
            // add check to see if other clients are allowed to create network classes
            int registerID = message.GetInt();
            string networkID = message.GetString();
            string json = message.GetString();
            Instance.LastUsedNetworkID = networkID;
            Instance._serverNetworkGameClassOwnership.Add(networkID, client);
            Message returnMessage = Message.Create(MessageSendMode.Reliable,
                (ushort)NetworkingMessageType.NetworkClassCreated);
            returnMessage.AddInt(registerID);
            returnMessage.AddString(networkID);
            returnMessage.AddUShort(client);
            returnMessage.AddString(json);
            Instance.Server.SendToAll(returnMessage);
        }

        [MessageHandler((ushort)NetworkingMessageType.NetworkClassUpdated)]
        private static void ServerReceiveNetworkClassUpdate(ushort client, Message message)
        {
            string networkID = message.GetString();
            uint updateVersion = message.GetUInt();
            ushort type = message.GetUShort();
            string json = message.GetString();

            if (Instance._serverNetworkGameClassOwnership[networkID] != client)
            {
                Debug.LogError($"Client {client} is trying to update a network game class with " +
                    $"id [{networkID}] that doesn't belong to them!");
                return;
            }

            Message returnMessage = Message.Create(MessageSendMode.Reliable, (ushort)NetworkingMessageType.NetworkClassUpdated);
            returnMessage.AddString(networkID);
            returnMessage.AddUInt(updateVersion);
            returnMessage.AddUShort(type);
            returnMessage.AddString(json);
            Instance.Server.SendToAll(returnMessage, client);
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
            Server.ClientConnected += ServerUpdateClientNetworkGameClassOnJoin;
            TimeServerStarted = DateTime.UtcNow.Ticks;
        }

        public void StopServer()
        {
            if (Server == null || !Server.IsRunning) return;

            Server.Stop();
            Server = null;
        }

        public void ServerUpdateClientNetworkGameClassOnJoin(object? sender, ServerConnectedEventArgs e)
        {
            ushort client = e.Client.Id;
            foreach(var pair in ClientNetworkGameClasses)
            {
                NetworkGameClass gameClass = pair.Value;
                Message message = Message.Create(MessageSendMode.Reliable, (ushort)NetworkingMessageType.NetworkClassCreated);
                message.AddInt(gameClass.GetRegisterID());
                message.AddString(gameClass.NetworkID);
                message.AddUShort(gameClass.OwnerID);
                message.AddString(gameClass.ToJson());
                Server.Send(message, client);
            }
        }
        #endregion

        #region Client
        [MessageHandler((ushort)NetworkingMessageType.NetworkClassCreated)]
        private static void ClientReceiveNetworkClassCreation(Message message)
        {
            int registerID = message.GetInt();
            string networkID = message.GetString();
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

        [MessageHandler((ushort)NetworkingMessageType.NetworkClassUpdated)]
        private static void ClientReceiveNetworkClassUpdate(Message message)
        {
            string networkID = message.GetString();
            uint updateVersion = message.GetUInt();
            ushort type = message.GetUShort();
            string json = message.GetString();

            NetworkGameClass networkGameClass = Instance.ClientNetworkGameClasses[networkID];
            if(networkGameClass.CheckUpdateVersion(type, updateVersion))
            {
                networkGameClass.ReceiveData(type, json);
            }
        }

        [MessageHandler((ushort)NetworkingMessageType.NetworkClassSync)]
        private static void ClientReceiveNetworkSyncRequest(Message message)
        {
            string networkID = message.GetString();
            uint updateVersion = message.GetUInt();
            ushort type = message.GetUShort();
            string json = message.GetString();

            NetworkGameClass networkGameClass = Instance.ClientNetworkGameClasses[networkID];
            if (networkGameClass.CheckUpdateVersion(type, updateVersion))
            {
                networkGameClass.ReceiveData(type, json);
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

        public void Disconnect()
        {
            Client.Disconnect();
            ClientNetworkGameClasses.Clear();
            StopServer();
            _serverNetworkGameClassOwnership.Clear();
            _serverPassword = "";
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