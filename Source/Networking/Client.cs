using Riptide;
using Riptide.Transports;
using Riptide.Transports.Steam;
using System.Collections.Concurrent;

namespace Electron2D.Networking.ClientServer
{
    /// <summary>
    /// A simple host/client client implementation using <see cref="Riptide.Client"/>. Not recommended to use 
    /// outside of <see cref="NetworkManager.Client"/>
    /// </summary>
    public class Client
    {
        public Riptide.Client RiptideClient { get; private set; }
        public SteamClient SteamClient { get; private set; }

        public Dictionary<string, NetworkGameClass> NetworkGameClasses { get; private set; } = new();
        public ushort ID => RiptideClient.Id;
        public bool IsConnected => RiptideClient.IsConnected;
        public bool IsConnecting => RiptideClient.IsConnecting;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event Action NetworkGameClassesLoaded;
        public event Action<string> ConnectionFailed;
        public event Action ConnectionSuccess;

        private Server _server;
        private ConcurrentQueue<(NetworkMessageType, Message)> _messageQueue = new();
        private bool _isSyncing = false;
        private bool _isPaused = false;
        private int _syncCount = 0;

        public Client(NetworkMode networkMode)
        {
            if (networkMode == NetworkMode.SteamP2P)
            {
                SteamClient = new SteamClient();
                RiptideClient = new Riptide.Client(SteamClient);
            }
            else
            {
                RiptideClient = new Riptide.Client();
            }
            RiptideClient.ConnectionFailed += HandleConnectionFailed;
            RiptideClient.Connected += HandleConnected;
            RiptideClient.Disconnected += HandleDisconnect;
            RiptideClient.MessageReceived += HandleMessageReceived;
        }

        /// <summary>
        /// Sets the server reference.
        /// </summary>
        /// <param name="server"></param>
        public void SetServer(Server server)
        {
            if (_server != null) return;
            _server = server;
            SteamClient.ChangeLocalServer(server.SteamServer);
        }

        /// <summary>
        /// Should be called at a fixed timestep.
        /// </summary>
        public void ClientFixedUpdate()
        {
            RiptideClient.Update();
        }

        /// <summary>
        /// Should be called as often as possible.
        /// </summary>
        public void ClientUpdate()
        {
            if(!_isPaused)
            {
                while(_messageQueue.Count > 0)
                {
                    (NetworkMessageType, Message) message;
                    if (!_messageQueue.TryDequeue(out message)) break;
                    if(_isSyncing && message.Item1 != NetworkMessageType.NetworkClassSync)
                    {
                        _messageQueue.Enqueue(message);
                        break;
                    }

                    switch (message.Item1)
                    {
                        case NetworkMessageType.NetworkClassSpawned:
                            HandleNetworkClassSpawned(message.Item2);
                            message.Item2.Release();
                            break;
                        case NetworkMessageType.NetworkClassUpdated:
                            HandleNetworkClassUpdated(message.Item2);
                            message.Item2.Release();
                            break;
                        case NetworkMessageType.NetworkClassDespawned:
                            HandleNetworkClassDespawned(message.Item2);
                            message.Item2.Release();
                            break;
                        case NetworkMessageType.NetworkClassSync:
                            HandleNetworkClassSync(message.Item2);
                            message.Item2.Release();
                            break;
                        case NetworkMessageType.NetworkClassRequestSyncData:
                            HandleNetworkClassRequestSyncData(message.Item2);
                            message.Item2.Release();
                            break;
                    }
                }
            }
        }

        public void Send(Message message, bool shouldRelease = true)
        {
            RiptideClient.Send(message, shouldRelease);
        }
        public bool Connect(string address, ushort port = 25565, string password = "")
        {
            if (IsConnected || IsConnecting) return false;

            Message message = Message.Create();
            message.AddString(password);
            if(SteamClient != null)
            {
                return RiptideClient.Connect($"{address}", message: message, useMessageHandlers: false);
            }
            else
            {
                return RiptideClient.Connect($"{address}:{port}", message: message, useMessageHandlers: false);
            }
        }
        public void Disconnect()
        {
            RiptideClient.Disconnect();
        }

        #region Handlers
        private void HandleMessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            if (e.MessageId < NetworkManager.MIN_NETWORK_MESSAGE_TYPE || e.MessageId > NetworkManager.MAX_NETWORK_MESSAGE_TYPE)
            {
                MessageReceived?.Invoke(sender, e);
                return;
            }

            Message message = Message.Create();
            message.AddMessage(e.Message);
            _messageQueue.Enqueue(((NetworkMessageType)e.MessageId, message));
        }
        private void HandleNetworkClassRequestSyncData(Message message)
        {
            // Server calls this on host client only
            _isPaused = true;
            ushort syncClient = message.GetUShort();
            if (syncClient == ID) return;
            Message returnMessage = Message.Create(MessageSendMode.Reliable,
                (ushort)NetworkMessageType.NetworkClassRequestSyncData);
            returnMessage.AddUShort(syncClient);
            int initializedClasses = 0;
            foreach (var gameClass in NetworkGameClasses.Values)
            {
                if(gameClass.IsNetworkInitialized)
                {
                    initializedClasses++;
                }
            }
            returnMessage.AddInt(initializedClasses);
            foreach (var gameClass in NetworkGameClasses.Values)
            {
                if (!gameClass.IsNetworkInitialized) continue;

                returnMessage.AddUInt(gameClass.UpdateVersion);
                returnMessage.AddInt(gameClass.GetRegisterID());
                returnMessage.AddString(gameClass.NetworkID);
                returnMessage.AddUShort(gameClass.OwnerID);
                returnMessage.AddString(gameClass.ToJson());
            }
            Debug.Log($"(CLIENT): Sending requested sync data to server, total count {initializedClasses}.");
            Send(returnMessage);
            _isPaused = false;
        }
        private void HandleNetworkClassSpawned(Message message)
        {
            uint version = message.GetUInt();
            int registerID = message.GetInt();
            string networkID = message.GetString();
            ushort clientID = message.GetUShort();
            string json = message.GetString();

            if (clientID == ID)
            {
                // Network game class was spawned by local player
                NetworkGameClasses[networkID].SetUpdateVersion(version);
                NetworkGameClasses[networkID].NetworkInitialize(networkID, clientID, this, _server);
            }
            else
            {
                NetworkGameClass networkGameClass = NetworkManager.NetworkGameClassRegister[registerID](json);
                networkGameClass.SetUpdateVersion(version);
                NetworkGameClasses.Add(networkID, networkGameClass);
                networkGameClass.NetworkInitialize(networkID, clientID, this, _server);
            }
        }
        private void HandleNetworkClassUpdated(Message message)
        {
            string networkID = message.GetString();
            uint updateVersion = message.GetUInt();
            ushort type = message.GetUShort();
            string json = message.GetString();

            NetworkGameClass networkGameClass = GetNetworkGameClass(networkID);
            if (networkGameClass != null)
            {
                if (networkGameClass.CheckUpdateVersion(type, updateVersion))
                {
                    networkGameClass.SetUpdateVersion(updateVersion);
                    networkGameClass.ReceiveData(type, json);
                }
            }
        }
        private void HandleNetworkClassDespawned(Message message)
        {
            string networkID = message.GetString();

            NetworkGameClass networkGameClass = GetNetworkGameClass(networkID);
            if (networkGameClass != null)
            {
                NetworkGameClasses.Remove(networkID);
                networkGameClass.Despawn(false);
            }
        }
        private void HandleNetworkClassSync(Message message)
        {
            if(_isSyncing)
            {
                // Receiving sync data
                HandleNetworkClassSpawned(message);
                _syncCount--;
                Debug.Log($"(CLIENT): Received sync data from server, {_syncCount} left.");
                if (_syncCount == 0)
                {
                    _isSyncing = false;
                    NetworkGameClassesLoaded?.Invoke();
                    Debug.Log("(CLIENT): Done syncing!");
                }
            }
            else
            {
                int syncCount = message.GetInt();
                _syncCount = syncCount;
                if(syncCount == 0)
                {
                    NetworkGameClassesLoaded?.Invoke();
                    Debug.Log("(CLIENT): Received sync signal from server with no data to sync, done syncing!");
                    return;
                }
                // Telling the server to start the sync
                _isSyncing = true;
                Debug.Log($"(CLIENT): Received sync signal from server, total count: {syncCount}. Asking server for data...");
                Send(Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageType.NetworkClassSync));
            }
        }
        private void HandleConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            ConnectionFailed?.Invoke(e.Message.GetString());
        }
        private void HandleConnected(object? sender, EventArgs e)
        {
            ConnectionSuccess?.Invoke();
        }
        private void HandleDisconnect(object? sender, EventArgs e)
        {
            foreach (var pair in NetworkGameClasses)
            {
                pair.Value.Despawn(false);
            }
            NetworkGameClasses.Clear();
            _messageQueue.Clear();
            _isPaused = false;
            _isSyncing = false;
            _syncCount = 0;
        }
        #endregion

        private NetworkGameClass GetNetworkGameClass(string networkID)
        {
            NetworkGameClasses.TryGetValue(networkID, out var gameClass);
            return gameClass;
        }
    }
}
