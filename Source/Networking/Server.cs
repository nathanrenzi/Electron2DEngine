using Riptide;
using Riptide.Transports;
using Riptide.Transports.Steam;

namespace Electron2D.Networking.ClientServer
{
    /// <summary>
    /// A simple host/client server implementation using <see cref="Riptide.Server"/>. Not recommended to use 
    /// outside of <see cref="NetworkManager.Server"/>
    /// </summary>
    public class Server
    {
        private class NetworkGameClassData
        {
            public uint UpdateVersion;
            public int RegisterID;
            public string NetworkID;
            public ushort OwnerID;
            public string JsonData;
        }

        public Riptide.Server RiptideServer { get; private set; }
        public SteamServer SteamServer { get; private set; }

        public long TimeStarted { get; private set; } = -1;
        public bool IsRunning => RiptideServer.IsRunning;
        public bool AllowNonHostOwnership { get; set; } = true;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private Dictionary<uint, List<NetworkGameClassData>> _syncingClientSnapshots = new();
        private Dictionary<string, ushort> _networkGameClassOwners = new();
        private List<string> _networkGameClassesToRemove = new();
        private List<(string, ushort)> _networkGameClassesToAdd = new();
        private bool _queueNetworkGameClasses = false;
        private bool _hostAssigned = false;
        private ushort _hostID = 1;
        private string _serverPassword = null;

        public Server(NetworkMode networkMode)
        {
            if(networkMode == NetworkMode.SteamP2P)
            {
                SteamServer = new SteamServer();
                RiptideServer = new Riptide.Server(SteamServer);
            }
            else
            {
                RiptideServer = new Riptide.Server();
            }
            RiptideServer.HandleConnection = ValidateConnection;
            RiptideServer.ClientConnected += HandleClientConnected;
            RiptideServer.ClientDisconnected += HandleClientDisconnected;
            RiptideServer.MessageReceived += HandleMessageReceived;
        }

        /// <summary>
        /// Should be called at a fixed timestep.
        /// </summary>
        public void ServerFixedUpdate()
        {
            RiptideServer.Update();
        }

        public void Send(Message message, ushort client, bool shouldRelease = true)
        {
            RiptideServer.Send(message, client, shouldRelease);
        }
        public void Send(Message message, Connection toClient, bool shouldRelease = true)
        {
            RiptideServer.Send(message, toClient, shouldRelease);
        }
        public void SendToAll(Message message, bool shouldRelease = true)
        {
            RiptideServer.SendToAll(message, shouldRelease);
        }
        public void SendToAll(Message message, ushort exceptToClient, bool shouldRelease = true)
        {
            RiptideServer.SendToAll(message, exceptToClient, shouldRelease);
        }

        public void Start(ushort port, ushort maxClientCount, string password = "")
        {
            if (IsRunning) return;
            _serverPassword = password;
            RiptideServer.Start(port, maxClientCount, useMessageHandlers: false);
            TimeStarted = DateTime.UtcNow.Ticks;
        }
        public void Stop()
        {
            if (!IsRunning) return;
            RiptideServer.Stop();
            _syncingClientSnapshots.Clear();
            _networkGameClassOwners.Clear();
            _serverPassword = "";
            _hostAssigned = false;
            _hostID = 0;
        }

        #region Handlers
        private void HandleMessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            if (e.MessageId < NetworkManager.MIN_NETWORK_MESSAGE_TYPE || e.MessageId > NetworkManager.MAX_NETWORK_MESSAGE_TYPE)
            {
                MessageReceived?.Invoke(sender, e);
                return;
            }

            ushort client = e.FromConnection.Id;
            switch((NetworkMessageType)e.MessageId)
            {
                case NetworkMessageType.NetworkClassSpawned:
                    HandleNetworkClassSpawned(client, e.Message);
                    break;
                case NetworkMessageType.NetworkClassUpdated:
                    HandleNetworkClassUpdated(client, e.Message);
                    break;
                case NetworkMessageType.NetworkClassDespawned:
                    HandleNetworkClassDespawned(client, e.Message);
                    break;
                case NetworkMessageType.NetworkClassSync:
                    HandleNetworkClassSync(client, e.Message);
                    break;
                case NetworkMessageType.NetworkClassRequestSyncData:
                    HandleNetworkClassRequestSyncData(client, e.Message);
                    break;
            }
        }
        private void HandleNetworkClassRequestSyncData(ushort client, Message message)
        {
            if(client != _hostID)
            {
                Debug.LogError($"(SERVER): Non-host client [{client}] tried to send client sync data to the server.");
                return;
            }

            Debug.Log("(SERVER): Received requested sync data from host. Asking client to sync...");
            ushort toClient = message.GetUShort();
            int classCount = message.GetInt();
            if(classCount > 0) _syncingClientSnapshots.Add(toClient, new List<NetworkGameClassData>());
            for (int i = 0; i < classCount; i++)
            {
                NetworkGameClassData data = new NetworkGameClassData();
                data.UpdateVersion = message.GetUInt();
                data.RegisterID = message.GetInt();
                data.NetworkID = message.GetString();
                data.OwnerID = message.GetUShort();
                data.JsonData = message.GetString();
                _syncingClientSnapshots[toClient].Add(data);
            }
            Message returnMessage = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageType.NetworkClassSync);
            returnMessage.AddInt(classCount);
            Send(returnMessage, toClient);
        }
        private void HandleNetworkClassSpawned(ushort client, Message message)
        {
            bool despawn = false;
            if (!AllowNonHostOwnership && client != _hostID)
            {
                Debug.LogWarning($"(SERVER): A non-host client [{client}] tried to spawn a network game class. Using the " +
                    $"current networking settings, this is not allowed.");
                despawn = true;
            }

            uint version = message.GetUInt();
            int registerID = message.GetInt();
            string networkID = message.GetString();
            string json = message.GetString();

            if (_networkGameClassOwners.ContainsKey(networkID) || despawn)
            {
                Debug.LogError($"(SERVER): Network game class with id [{networkID}] already exists on the server. Cannot spawn.");
                Message despawnMessage = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageType.NetworkClassDespawned);
                despawnMessage.AddString(networkID);
                Send(despawnMessage, client);
                return;
            }

            if(_queueNetworkGameClasses)
            {
                _networkGameClassesToAdd.Add((networkID, client));
            }
            else
            {
                _networkGameClassOwners.Add(networkID, client);
            }

            Message returnMessage = Message.Create(MessageSendMode.Reliable,
                (ushort)NetworkMessageType.NetworkClassSpawned);
            returnMessage.AddUInt(version);
            returnMessage.AddInt(registerID);
            returnMessage.AddString(networkID);
            returnMessage.AddUShort(client);
            returnMessage.AddString(json);
            SendToAll(returnMessage);
        }
        private void HandleNetworkClassUpdated(ushort client, Message message)
        {
            string networkID = message.GetString();
            uint updateVersion = message.GetUInt();
            ushort type = message.GetUShort();
            string json = message.GetString();

            // Checking if client is owner of object
            if (_networkGameClassOwners[networkID] != client)
            {
                Debug.LogWarning($"(SERVER): Client {client} is trying to update a network game class with " +
                    $"id [{networkID}] that doesn't belong to them!");
                return;
            }

            Message returnMessage = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageType.NetworkClassUpdated);
            returnMessage.AddString(networkID);
            returnMessage.AddUInt(updateVersion);
            returnMessage.AddUShort(type);
            returnMessage.AddString(json);
            SendToAll(returnMessage, client);
        }
        private void HandleNetworkClassSync(ushort client, Message message)
        {
            if (!_syncingClientSnapshots.ContainsKey(client))
            {
                Debug.LogError($"(SERVER): Client [{client}] tried to get sync data from server without permission.");
                return;
            }

            Debug.Log($"(SERVER): Received sync confirmation from client {client}. Sending data...");
            List<NetworkGameClassData> dataList = _syncingClientSnapshots[client];
            foreach (var data in dataList)
            {
                Message returnMessage = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageType.NetworkClassSync);
                returnMessage.AddUInt(data.UpdateVersion);
                returnMessage.AddInt(data.RegisterID);
                returnMessage.AddString(data.NetworkID);
                returnMessage.AddUShort(data.OwnerID);
                returnMessage.AddString(data.JsonData);
                Send(returnMessage, client);
            }
            _syncingClientSnapshots.Remove(client);
        }
        private void HandleNetworkClassDespawned(ushort client, Message message, bool exceptClient = false)
        {
            string networkID = message.GetString();

            if (_networkGameClassOwners[networkID] != client)
            {
                Debug.LogWarning($"(SERVER): Client {client} tried to despawn network game class with id [{networkID}] which doesn't belong " +
                    "to them!");
                return;
            }
            
            if(_queueNetworkGameClasses)
            {
                _networkGameClassesToRemove.Add(networkID);
            }
            else
            {
                _networkGameClassOwners.Remove(networkID);
            }
            Message returnMessage = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageType.NetworkClassDespawned);
            returnMessage.AddString(networkID);
            if(exceptClient)
            {
                // If this client just disconnected, excluding them from message
                SendToAll(returnMessage, client);
            }
            else
            {
                SendToAll(returnMessage);
            }
        }
        private void HandleClientConnected(object? sender, ServerConnectedEventArgs e)
        {
            if(e.Client.Id == _hostID)
            {
                Message hostInitializeMessage = Message.Create(MessageSendMode.Reliable,
                    (ushort)NetworkMessageType.NetworkClassSync);
                hostInitializeMessage.AddInt(0);
                Send(hostInitializeMessage, _hostID);
                return;
            }

            Debug.Log("(SERVER): Client joined, sending sync signal.");
            Message toHostMessage = Message.Create(MessageSendMode.Reliable,
                (ushort)NetworkMessageType.NetworkClassRequestSyncData);
            toHostMessage.AddUShort(e.Client.Id);
            Send(toHostMessage, _hostID);
        }
        private void HandleClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
        {
            if(e.Client.Id == _hostID)
            {
                Debug.Log("(SERVER): Until host transferring is implemented, the server will stop when the host leaves.");
                Stop();
                return;
            }

            _queueNetworkGameClasses = true;
            foreach (var pair in _networkGameClassOwners)
            {
                if(pair.Value == e.Client.Id)
                {
                    // Delete objects
                    Message message = Message.Create();
                    message.AddString(pair.Key);
                    HandleNetworkClassDespawned(e.Client.Id, message, true);
                    message.Release();
                }
            }
            _queueNetworkGameClasses = false;
            PopQueues();
        }
        #endregion

        private void PopQueues()
        {
            if (_queueNetworkGameClasses) return;
            for (int i = 0; i < _networkGameClassesToAdd.Count; i++)
            {
                _networkGameClassOwners.Add(_networkGameClassesToAdd[i].Item1, _networkGameClassesToAdd[i].Item2);
            }
            _networkGameClassesToAdd.Clear();
            for (int i = 0; i < _networkGameClassesToRemove.Count; i++)
            {
                _networkGameClassOwners.Remove(_networkGameClassesToRemove[i]);
            }
            _networkGameClassesToRemove.Clear();
        }
        private void ValidateConnection(Connection pendingConnection, Message connectMessage)
        {
            string password = connectMessage.GetString();
            if (_serverPassword != "")
            {
                if (_serverPassword.Equals(password))
                {
                    RiptideServer.Accept(pendingConnection);
                }
                else
                {
                    RiptideServer.Reject(pendingConnection, Message.Create().AddString("Incorrect password."));
                }
            }

            RiptideServer.Accept(pendingConnection);
        }
    }
}
