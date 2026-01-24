using Riptide;
using Riptide.Transports.Steam;

namespace Electron2D.Networking.Core
{
    /// <summary>
    /// A host/client client implementation using <see cref="Riptide.Client"/>.
    /// </summary>
    public sealed class Client : IDisposable
    {
        public Riptide.Client RiptideClient { get; private set; }
        public SteamClient SteamClient { get; private set; }
        public NetworkServiceManager Services { get; private set; } = new(false);
        public Dictionary<string, NetworkGameClass> NetworkGameClasses { get; private set; } = new();
        public ushort ID => RiptideClient.Id;
        public bool IsConnected => RiptideClient.IsConnected;
        public bool IsConnecting => RiptideClient.IsConnecting;

        public event Action NetworkGameClassesLoaded;
        public event Action<RejectReason> ConnectionFailed;
        public event Action ConnectionSuccessful;
        public event Action<DisconnectReason> Disconnected;
        public event Action<ushort> ClientConnected;
        public event Action<ushort> ClientDisconnected;
        public event Action<string> NetworkGameClassSpawned;

        private Queue<(NetworkGameClass, string, ushort)> _syncingNetworkGameClasses = new();
        private Server _server;
        private Queue<(BuiltInMessageType, object)> _messageQueue = new();
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
        }

        ~Client()
        {
            Dispose();
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
            RiptideClient?.Update();
            if (!_isPaused)
            {
                while(_messageQueue.Count > 0)
                {
                    (BuiltInMessageType, object) data;
                    if (!_messageQueue.TryDequeue(out data)) break;
                    if(_isSyncing && data.Item1 != BuiltInMessageType.NetworkClassSync)
                    {
                        _messageQueue.Enqueue(data);
                        break;
                    }

                    switch (data.Item1)
                    {
                        case BuiltInMessageType.NetworkClassSpawned:
                            HandleNetworkClassSpawned((NetworkGameClassData)data.Item2);
                            break;
                        case BuiltInMessageType.NetworkClassUpdated:
                            HandleNetworkClassUpdated((NetworkGameClassUpdatedData)data.Item2);
                            break;
                        case BuiltInMessageType.NetworkClassDespawned:
                            HandleNetworkClassDespawned((string)data.Item2);
                            break;
                        case BuiltInMessageType.NetworkClassSync:
                            if(_isSyncing)
                            {
                                HandleNetworkClassSyncSpawn((NetworkGameClassSyncSpawnData)data.Item2);
                            }
                            else
                            {
                                HandleNetworkClassSyncStart((int)data.Item2);
                            }
                            break;
                        case BuiltInMessageType.NetworkClassRequestSyncData:
                            HandleNetworkClassRequestSyncData((ushort)data.Item2);
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
                return RiptideClient.Connect($"{(address == "localhost" ? "127.0.0.1" : address)}:{port}", message: message, useMessageHandlers: false);
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
            if (e.MessageId < Network.MIN_MESSAGE_TYPE_INTERCEPT || e.MessageId > Network.MAX_MESSAGE_TYPE_INTERCEPT)
            {
                Services.Dispatch(e.MessageId, e.Message);
                return;
            }

            BuiltInMessageType messageType = (BuiltInMessageType)e.MessageId;
            Message message = e.Message;
            object data = null;
            switch (messageType)
            {
                case BuiltInMessageType.NetworkClassSpawned:
                    data = new NetworkGameClassData()
                    {
                        Version = message.GetUInt(),
                        RegisterID = message.GetInt(),
                        NetworkID = message.GetString(),
                        OwnerID = message.GetUShort(),
                        Json = message.GetString()
                    };
                    break;
                case BuiltInMessageType.NetworkClassUpdated:
                    data = new NetworkGameClassUpdatedData()
                    {
                        NetworkID = message.GetString(),
                        Version = message.GetUInt(),
                        Type = message.GetUShort(),
                        Json = message.GetString()
                    };
                    break;
                case BuiltInMessageType.NetworkClassDespawned:
                    data = message.GetString();
                    break;
                case BuiltInMessageType.NetworkClassSync:
                    if (_isSyncing)
                    {
                        data = new NetworkGameClassSyncSpawnData()
                        {
                            Version = message.GetUInt(),
                            RegisterID = message.GetInt(),
                            NetworkID = message.GetString(),
                            ClientID = message.GetUShort(),
                            Json = message.GetString()
                        };
                    }
                    else
                    {
                        data = message.GetInt();
                    }
                    break;
                case BuiltInMessageType.NetworkClassRequestSyncData:
                    data = message.GetUShort();
                    break;
            }
            _messageQueue.Enqueue((messageType, data));
        }
        private void HandleNetworkClassRequestSyncData(ushort client)
        {
            // Server calls this on host client only
            _isPaused = true;
            if (client == ID) return;
            Message returnMessage = Message.Create(MessageSendMode.Reliable,
                (ushort)BuiltInMessageType.NetworkClassRequestSyncData);
            returnMessage.AddUShort(client);
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
        private void HandleNetworkClassSpawned(NetworkGameClassData data)
        {
            if (data.OwnerID == ID)
            {
                // Network game class was spawned by local player
                if (!NetworkGameClasses.ContainsKey(data.NetworkID))
                {
                    Debug.LogError($"(CLIENT): Could not initialize locally spawned NetworkGameClass with id [{data.NetworkID}]. This should not happen.");
                    return;
                }
                NetworkGameClasses[data.NetworkID].SetUpdateVersion(data.Version);
                NetworkGameClasses[data.NetworkID].NetworkInitialize(data.NetworkID, data.OwnerID, this, _server);
            }
            else
            {
                NetworkGameClass networkGameClass = Network.NetworkGameClassRegister[data.RegisterID](data.Json);
                networkGameClass.SetUpdateVersion(data.Version);
                NetworkGameClasses.Add(data.NetworkID, networkGameClass);
                networkGameClass.NetworkInitialize(data.NetworkID, data.OwnerID, this, _server);
            }

            NetworkGameClassSpawned?.Invoke(data.NetworkID);
        }
        private void HandleNetworkClassUpdated(NetworkGameClassUpdatedData data)
        {
            NetworkGameClass networkGameClass = GetNetworkGameClass(data.NetworkID);
            if (networkGameClass != null)
            {
                if (networkGameClass.CheckAndHandleUpdateVersion(data.Type, data.Version))
                {
                    networkGameClass.ReceiveData(data.Type, data.Json);
                }
            }
        }
        private void HandleNetworkClassDespawned(string networkID)
        {
            NetworkGameClass networkGameClass = GetNetworkGameClass(networkID);
            if (networkGameClass != null)
            {
                NetworkGameClasses.Remove(networkID);
                networkGameClass.Despawn(false);
            }
        }
        private void HandleNetworkClassSyncSpawn(NetworkGameClassSyncSpawnData data)
        {
            if (data.ClientID == ID)
            {
                // Network game class was spawned by local player
                if (!NetworkGameClasses.ContainsKey(data.NetworkID))
                {
                    Debug.LogError($"(CLIENT): Could not initialize locally spawned NetworkGameClass with id [{data.NetworkID}]. This should not happen.");
                    return;
                }
                NetworkGameClasses[data.NetworkID].SetUpdateVersion(data.Version);
                _syncingNetworkGameClasses.Enqueue((NetworkGameClasses[data.NetworkID], data.NetworkID, data.ClientID));
            }
            else
            {
                NetworkGameClass networkGameClass = Network.NetworkGameClassRegister[data.RegisterID](data.Json);
                networkGameClass.SetUpdateVersion(data.Version);
                NetworkGameClasses.Add(data.NetworkID, networkGameClass);
                _syncingNetworkGameClasses.Enqueue((networkGameClass, data.NetworkID, data.ClientID));
            }
            _syncCount--;
            Debug.Log($"(CLIENT): Received sync data from server, {_syncCount} left.");
            if (_syncCount == 0)
            {
                _isSyncing = false;
                while (_syncingNetworkGameClasses.Count != 0)
                {
                    (NetworkGameClass, string, ushort) gameClassData = _syncingNetworkGameClasses.Dequeue();
                    gameClassData.Item1.NetworkInitialize(gameClassData.Item2, gameClassData.Item3, this, _server);
                    NetworkGameClassSpawned?.Invoke(gameClassData.Item2);
                }
                NetworkGameClassesLoaded?.Invoke();
                Debug.Log("(CLIENT): Done syncing!");
            }
        }
        private void HandleNetworkClassSyncStart(int syncCount)
        {
            _syncCount = syncCount;
            if (syncCount == 0)
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
        private void HandleConnectionFailed(object? sender, ConnectionFailedEventArgs e)
        {
            ConnectionFailed?.Invoke(e.Reason);
        }
        private void HandleConnected(object? sender, EventArgs e)
        {
            ConnectionSuccessful?.Invoke();
        }
        private void HandleDisconnected(object? sender, DisconnectedEventArgs e)
        {
            foreach (var pair in NetworkGameClasses)
            {
                if(!pair.Value.IsOwner)
                {
                    pair.Value.MarkDispose();
                }
                else
                {
                    pair.Value.Despawn(true);
                }
            }
            NetworkGameClasses.Clear();
            _syncingNetworkGameClasses.Clear();
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
            Disconnect();
        }
    }
}
