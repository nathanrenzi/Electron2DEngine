using Riptide;

namespace Electron2D.Networking
{
    /// <summary>
    /// An abstract class that provides the boilerplate for networked game classes.
    /// Abstract method <see cref="ReceiveData(ushort, string)"/> receives update data
    /// from the server and interprets it. <see cref="Send(MessageSendMode, string, ushort)"/>
    /// sends update data to the server, which is then received and interpreted by all other clients.
    /// <see cref="ToJson()"/> is called by a message from the server, requesting the current
    /// state of the game class so that a connecting client can properly initialize their object. Any
    /// subclasses of <see cref="NetworkGameClass"/> must call <see cref="NetworkManager.RegisterNetworkGameClass"/>
    /// in the <see cref="Game.Initialize"/> method to be properly instantiated over the network.
    /// </summary>
    public abstract class NetworkGameClass : IGameClass
    {
        private struct DependencyCallback
        {
            public Type ExpectedType;
            public Action<NetworkGameClass> Callback;
        }

        public string NetworkID { get; private set; } = string.Empty;
        public ushort OwnerID { get; private set; } = ushort.MaxValue;
        public bool IsOwner { get; private set; } = false;
        public bool IsNetworkInitialized { get; private set; }
        public uint UpdateVersion { get; private set; } = 0;
        public bool RemoveLocallyOnDespawn { get; set; }

        public event Action OnNetworkInitializedEvent;
        public event Action OnNetworkDespawnedEvent;

        protected ClientServer.Client _client;
        protected ClientServer.Server _server;

        private Dictionary<string, DependencyCallback> _dependencies = new Dictionary<string, DependencyCallback>();
        private bool _hasAddedDependencies = false;
        private bool _markDispose = false;

        public NetworkGameClass()
        {
            Engine.Game.RegisterGameClass(this);
            Engine.Game.LateUpdateEvent += () =>
            {
                if (_markDispose)
                {
                    Dispose();
                }
            };
        }

        ~NetworkGameClass()
        {
            Dispose();
        }

        /// <summary>
        /// Removes the object server-side and client-side. Calls <see cref="Despawn"/>.
        /// </summary>
        public void Dispose()
        {
            RemoveLocallyOnDespawn = true;
            Despawn();
        }
        /// <summary>
        /// Marks the object for disposal. Safe to use when disposing from another thread.
        /// </summary>
        public void MarkDispose()
        {
            _markDispose = true;
        }
        public abstract void FixedUpdate();
        public abstract void Update();

        /// <summary>
        /// Sends a request to the server to spawn this object.
        /// </summary>
        public void Spawn(string networkID = null, bool removeLocallyOnDespawn = false,
            ClientServer.Client customClient = null, ClientServer.Server customServer = null)
        {
            if (IsNetworkInitialized) return;
            if (customClient != null)
            {
                _client = customClient;
            }
            else
            {
                _client = NetworkManager.Instance.Client;
            }

            if (customServer != null)
            {
                _server = customServer;
            }
            else
            {
                _server = NetworkManager.Instance.Server;
            }

            if(string.IsNullOrEmpty(networkID))
            {
                networkID = Guid.NewGuid().ToString("N");
            }

            RemoveLocallyOnDespawn = removeLocallyOnDespawn;

            if (!_client.IsConnected)
            {
                Debug.LogError($"Trying to spawn network game class with id [{networkID}] before client is connected!");
                return;
            }
            if (_client.NetworkGameClasses.ContainsKey(networkID))
            {
                Debug.LogError($"Network game class with id [{networkID}] already exists on the client. Cannot spawn.");
                return;
            }

            OwnerID = _client.ID;
            IsOwner = true;
            NetworkID = networkID;
            _client.NetworkGameClasses.Add(NetworkID, this);

            Message message = Message.Create(MessageSendMode.Reliable,
                (ushort)BuiltInMessageType.NetworkClassSpawned);
            message.AddUInt(UpdateVersion);
            message.AddInt(GetRegisterID());
            message.AddString(networkID);
            message.AddString(ToJson());
            _client.Send(message);
        }
        /// <summary>
        /// Sends a request to the server to despawn this object (if owned by local player). It will still exist client-side.
        /// </summary>
        public void Despawn(bool sendMessageToServer = true)
        {
            if (!IsNetworkInitialized) return;
            IsNetworkInitialized = false;
            if(IsOwner && sendMessageToServer)
            {
                Message message = Message.Create(MessageSendMode.Reliable,
                    (ushort)BuiltInMessageType.NetworkClassDespawned);
                message.AddString(NetworkID);
                _client.Send(message);
                OnNetworkDespawnedEvent?.Invoke();
                OnDespawned();
                Reset();
                if (RemoveLocallyOnDespawn)
                {
                    Engine.Game.UnregisterGameClass(this);
                    GC.SuppressFinalize(this);
                    OnDisposed();
                }
            }
            else if(RemoveLocallyOnDespawn)
            {
                OnNetworkDespawnedEvent?.Invoke();
                OnDespawned();
                Reset();
                Engine.Game.UnregisterGameClass(this);
                GC.SuppressFinalize(this);
                OnDisposed();
            }
            else
            {
                OnNetworkDespawnedEvent?.Invoke();
                OnDespawned();
                Reset();
            }
        }
        /// <summary>
        /// Sends data to the server registered under this object's NetworkID.
        /// </summary>
        /// <param name="sendMode"></param>
        /// <param name="json"></param>
        /// <param name="type">The type of data being sent, in case of multiple update types.
        /// Leave default if only one update type is ever sent.</param>
        protected void Send(MessageSendMode sendMode, string json, ushort type = 0)
        {
            if (!IsOwner)
            {
                Debug.LogError("Cannot send data using NetworkGameClass that does not belong to the client.");
                return;
            }
            if (!IsNetworkInitialized)
            {
                Debug.LogWarning("Trying to send a message from a NetworkGameClass that has not been initialized yet.");
                return;
            }
            UpdateVersion++;
            Message message = Message.Create(sendMode, (ushort)BuiltInMessageType.NetworkClassUpdated);
            message.AddString(NetworkID);
            message.AddUInt(UpdateVersion);
            message.AddUShort(type);
            message.AddString(json);
            _client.Send(message);
        }
        /// <summary>
        /// Called by the server when spawning over the network. Should not be called elsewhere.
        /// </summary>
        /// <param name="networkID"></param>
        /// <param name="ownerID"></param>
        /// <param name="json"></param>
        public void NetworkInitialize(string networkID, ushort ownerID, ClientServer.Client client, ClientServer.Server server)
        {
            if (IsNetworkInitialized) return;

            _client = client;
            _server = server;
            if (!(server.IsRunning && client.IsConnected))
            {
                NetworkID = networkID;
                OwnerID = ownerID;
                IsOwner = client.ID == ownerID;
            }
            IsNetworkInitialized = true;
            OnNetworkInitializedEvent?.Invoke();
            OnNetworkInitialized();
        }
        /// <summary>
        /// Sets the global update version for this object.
        /// </summary>
        /// <param name="newVersion">Must be newer than the current update version.</param>
        public void SetUpdateVersion(uint newVersion)
        {
            if (newVersion > UpdateVersion)
            {
                UpdateVersion = newVersion;
            }
        }
        /// <summary>
        /// Adds a <see cref="NetworkGameClass"/> dependency, and runs a method once the specified NetworkGameClass is initialized.
        /// </summary>
        /// <param name="networkID">The networkID of the dependency.</param>
        /// <param name="onNetworkIDInitialized">The method that will run when the dependency is met.</param>
        protected void AddDependency<T>(string networkID, Action<T> onNetworkIDInitialized) where T : NetworkGameClass
        {
            if (!_hasAddedDependencies)
            {
                NetworkManager.Instance.Client.NetworkGameClassSpawned += CheckDependency;
                _hasAddedDependencies = true;
            }
            _dependencies.Add(networkID, new DependencyCallback
            {
                ExpectedType = typeof(T),
                Callback = obj => onNetworkIDInitialized((T)obj)
            });
            if (NetworkManager.Instance.Client.NetworkGameClasses.ContainsKey(networkID))
            {
                CheckDependency(networkID);
            }
        }
        private void CheckDependency(string networkID)
        {
            if (_dependencies.ContainsKey(networkID))
            {
                NetworkGameClass networkGameClass = NetworkManager.Instance.Client.GetNetworkGameClass(networkID);
                DependencyCallback callback = _dependencies[networkID];
                if(callback.ExpectedType.IsInstanceOfType(networkGameClass))
                {
                    callback.Callback(networkGameClass);
                }
                else
                {
                    Debug.LogError($"Dependency with ID: [{networkID}] expected type {callback.ExpectedType}, got type {networkGameClass.GetType()} in NetworkGameClass with ID: [{NetworkID}]");
                }
                _dependencies.Remove(networkID);
            }
        }
        /// <summary>
        /// Should initialize the network object from json data. Json should contain
        /// all data, including any data that has been changed since the object has been created
        /// so that players joining after object has been created are up to date.
        /// </summary>
        /// <param name="json">The json that is used to create the network class.</param>
        protected abstract void SetJson(string json);
        /// <summary>
        /// Should get the current state of the network class in json format.
        /// </summary>
        /// <returns>The current state of the object in json format.</returns>
        public abstract string ToJson();
        /// <summary>
        /// Should interpret the json data received from the server.
        /// </summary>
        /// <param name="type">The type of data received.</param>
        /// <param name="json">The update data in json format.</param>
        public abstract void ReceiveData(ushort type, string json);
        /// <summary>
        /// Returns the register ID of the class. The register ID must be set by passing the value returned
        /// from <see cref="NetworkManager.RegisterNetworkGameClass"/> into a static method in each subclass.
        /// </summary>
        /// <returns></returns>
        public abstract int GetRegisterID();
        /// <summary>
        /// Called when the server initializes the network game class.
        /// </summary>
        public abstract void OnNetworkInitialized();
        /// <summary>
        /// Called when the server despawns the network game class.
        /// </summary>
        public abstract void OnDespawned();
        /// <summary>
        /// Called when this object is disposed.
        /// </summary>
        public abstract void OnDisposed();
        /// <summary>
        /// Should check the received version against the stored one, either in <see cref="UpdateVersion"/> or in a custom version variable,
        /// optionally using the type parameter.
        /// Can return true to ignore update versions.
        /// </summary>
        /// <param name="type">The type of update received.</param>
        /// <param name="version">The version number for the update received.</param>
        /// <returns></returns>
        public abstract bool CheckAndHandleUpdateVersion(ushort type, uint version);

        private void Reset()
        {
            _client = null;
            IsOwner = false;
            OwnerID = 0;
            NetworkID = "";
            UpdateVersion = 0;
        }
    }
}
