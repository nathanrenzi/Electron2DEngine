using Riptide;
using Riptide.Transports.Steam;
using System.Collections.Concurrent;

namespace Electron2D.Networking.ClientServer
{
    /// <summary>
    /// A simple host/client client implementation using <see cref="Riptide.Client"/>. Not recommended to use 
    /// outside of <see cref="NetworkManager.Client"/>
    /// </summary>
    public class Client : IDisposable
    {
        public Riptide.Client RiptideClient { get; private set; }
        public SteamClient SteamClient { get; private set; }

        public Dictionary<string, NetworkGameClass> NetworkGameClasses { get; private set; } = new();
        public ushort ID => RiptideClient.Id;
        public bool IsConnected => RiptideClient.IsConnected;
        public bool IsConnecting => RiptideClient.IsConnecting;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event Action NetworkGameClassesLoaded;
        public event Action<RejectReason> ConnectionFailed;
        public event Action Connected;
        public event Action<DisconnectReason> Disconnected;
        public event Action<ushort> ClientConnected;
        public event Action<ushort> ClientDisconnected;
        public event Action<string> NetworkGameClassSpawned;

        private Thread _clientThread;
        private CancellationTokenSource _clientCancellationTokenSource;
        private Server _server;
        private ConcurrentQueue<(BuiltInMessageType, Message)> _messageQueue = new();
        private bool _isSyncing = false;
        private bool _isPaused = false;
        private int _syncCount = 0;
        private NetworkMode _networkMode;

        public Client(NetworkMode networkMode)
        {
            _networkMode = networkMode;
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
            RiptideClient.Disconnected += HandleDisconnected;
            RiptideClient.MessageReceived += HandleMessageReceived;
            RiptideClient.ClientConnected += (obj, e) => ClientConnected?.Invoke(e.Id);
            RiptideClient.ClientDisconnected += (obj, e) => ClientDisconnected?.Invoke(e.Id);

            _clientCancellationTokenSource = new CancellationTokenSource();
            _clientThread = new Thread(() => ClientThreadUpdateLoop(_clientCancellationTokenSource.Token));
            _clientThread.Priority = ThreadPriority.Highest;
            _clientThread.Start();
        }

        ~Client()
        {
            Dispose();
        }

        private void ClientThreadUpdateLoop(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                RiptideClient.Update();
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Sets the server reference.
        /// </summary>
        /// <param name="server"></param>
        public void SetServer(Server server)
        {
            if (_server != null) return;
            _server = server;
            SteamClient?.ChangeLocalServer(server.SteamServer);
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
                    (BuiltInMessageType, Message) message;
                    if (!_messageQueue.TryDequeue(out message)) break;
                    if(_isSyncing && message.Item1 != BuiltInMessageType.NetworkClassSync)
                    {
                        _messageQueue.Enqueue(message);
                        break;
                    }

                    switch (message.Item1)
                    {
                        case BuiltInMessageType.NetworkClassSpawned:
                            HandleNetworkClassSpawned(message.Item2);
                            message.Item2.Release();
                            break;
                        case BuiltInMessageType.NetworkClassUpdated:
                            HandleNetworkClassUpdated(message.Item2);
                            message.Item2.Release();
                            break;
                        case BuiltInMessageType.NetworkClassDespawned:
                            HandleNetworkClassDespawned(message.Item2);
                            message.Item2.Release();
                            break;
                        case BuiltInMessageType.NetworkClassSync:
                            HandleNetworkClassSync(message.Item2);
                            message.Item2.Release();
                            break;
                        case BuiltInMessageType.NetworkClassRequestSyncData:
                            HandleNetworkClassRequestSyncData(message.Item2);
                            message.Item2.Release();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Sends a message to the host server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="shouldRelease">Should the message be automatically released to the message pool after being sent?
        /// If you intend to use the message after it is sent, set this to false and use <see cref="Message.Release()"/> when done.</param>
        public void Send(Message message, bool shouldRelease = true)
        {
            RiptideClient.Send(message, shouldRelease);
        }

        /// <summary>
        /// Connects to a host using the given address and port. Note: Port is only used when the client is set to <see cref="NetworkMode.NetworkP2P"/>
        /// when created, <see cref="ProjectSettings.SteamPort"/> is used for <see cref="NetworkMode.SteamP2P"/>.
        /// </summary>
        /// <param name="address">The IP/SteamID of the host (depending on which NetworkMode is used).</param>
        /// <param name="port">The port to be used. Note: Only used for <see cref="NetworkMode.NetworkP2P"/>.</param>
        /// <param name="password">The password to be given to the host for validation.</param>
        /// <returns></returns>
        public bool Connect(string address, ushort port = 25565, string password = "")
        {
            if (IsConnected || IsConnecting)
            {
                Debug.LogError("Client cannot connect, already connected to a server.");
                return false;
            }

            Message message = Message.Create();
            message.AddString(password);
            if(_networkMode == NetworkMode.NetworkP2P)
            {
                return RiptideClient.Connect($"{address}:{port}", message: message, useMessageHandlers: false);
            }
            else
            {
                SteamClient.SetLastUsedPassword(password);
                if(address == "localhost" || address == "127.0.0.1")
                {
                    return RiptideClient.Connect($"{address}", message: message, useMessageHandlers: false);
                }
                else
                {
                    return RiptideClient.Connect($"{address}:{ProjectSettings.SteamPort}", message: message, useMessageHandlers: false);
                }
            }
        }

        /// <summary>
        /// Disconnects from the server that the client is currently connected to.
        /// </summary>
        public void Disconnect()
        {
            RiptideClient.Disconnect();
            _messageQueue.Clear();
            if(_networkMode == NetworkMode.SteamP2P)
            {
                SteamClient.SetLastUsedPassword("");
            }
        }

        /// <summary>
        /// Retrieves the <see cref="NetworkGameClass"> with the given NetworkID.
        /// </summary>
        /// <param name="networkID"></param>
        /// <returns></returns>
        public NetworkGameClass GetNetworkGameClass(string networkID)
        {
            if(NetworkGameClasses.ContainsKey(networkID))
            {
                return NetworkGameClasses[networkID];
            }
            else
            {
                Debug.LogError($"NetworkGameClass with ID: [{networkID}] does not exist!");
                return null;
            }
        }

        /// <summary>
        /// Attempts to retrieve the <see cref="NetworkGameClass"> with the given NetworkID.
        /// </summary>
        /// <param name="networkID"></param>
        /// <param name="networkGameClass"></param>
        /// <returns></returns>
        public bool TryGetNetworkGameClass(string networkID, out NetworkGameClass networkGameClass)
        {
            return NetworkGameClasses.TryGetValue(networkID, out networkGameClass);
        }

        #region Steam Methods
        /// <summary>
        /// If steam networking is being used, opens the steam invite menu.
        /// </summary>
        public void SteamOpenInviteMenu()
        {
            if(_networkMode != NetworkMode.SteamP2P)
            {
                Debug.LogError("Cannot open steam invite menu when steam networking is not currently being used!");
                return;
            }
            SteamClient.OpenInviteMenu();
        }

        /// <summary>
        /// If steam networking is being used, invites a friend.
        /// </summary>
        /// <param name="steamIDFriend">The steam ID of the friend to invite.</param>
        public void SteamInviteFriend(Steamworks.CSteamID steamIDFriend)
        {
            if (_networkMode != NetworkMode.SteamP2P)
            {
                Debug.LogError("Cannot invite steam friend when steam networking is not currently being used!");
                return;
            }
            SteamClient.InviteFriend(steamIDFriend);
        }
        #endregion

        #region Handlers
        private void HandleMessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            if (e.MessageId < NetworkManager.MIN_MESSAGE_TYPE_INTERCEPT || e.MessageId > NetworkManager.MAX_MESSAGE_TYPE_INTERCEPT)
            {
                MessageReceived?.Invoke(sender, e);
                return;
            }

            Message message = Message.Create();
            message.AddMessage(e.Message);
            _messageQueue.Enqueue(((BuiltInMessageType)e.MessageId, message));
        }
        private void HandleNetworkClassRequestSyncData(Message message)
        {
            // Server calls this on host client only
            _isPaused = true;
            ushort syncClient = message.GetUShort();
            if (syncClient == ID) return;
            Message returnMessage = Message.Create(MessageSendMode.Reliable,
                (ushort)BuiltInMessageType.NetworkClassRequestSyncData);
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

            NetworkGameClassSpawned?.Invoke(networkID);
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
                if (networkGameClass.CheckAndHandleUpdateVersion(type, updateVersion))
                {
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
                Send(Message.Create(MessageSendMode.Reliable, (ushort)BuiltInMessageType.NetworkClassSync));
            }
        }
        private void HandleConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            ConnectionFailed?.Invoke(e.Reason);
        }
        private void HandleConnected(object? sender, EventArgs e)
        {
            Connected?.Invoke();
        }
        private void HandleDisconnected(object? sender, DisconnectedEventArgs e)
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
            Disconnected?.Invoke(e.Reason);
        }
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _clientCancellationTokenSource.Cancel();
            _clientThread.Join();
            Disconnect();
        }
    }
}
