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
        public string NetworkID { get; private set; } = string.Empty;
        public ushort OwnerID { get; private set; } = ushort.MaxValue;
        public bool IsOwner { get; private set; } = false;
        public bool IsNetworkInitialized { get; private set; }
        public uint UpdateVersion { get; private set; } = 0;

        public NetworkGameClass(string networkID = "")
        {
            if(networkID != string.Empty)
            {
                Spawn(networkID);
            }
        }

        ~NetworkGameClass()
        {
            Dispose();
        }

        public void Dispose()
        {
            if(IsOwner)
            {
                Message message = Message.Create(MessageSendMode.Reliable,
                    (ushort)NetworkMessageType.NetworkClassDeleted);
                NetworkManager.Instance.Client.Send(message);
                Program.Game.UnregisterGameClass(this);
                GC.SuppressFinalize(this);
            }
        }
        public abstract void FixedUpdate();
        public abstract void Update();


        /// <summary>
        /// Sends a request to the server to spawn this object.
        /// </summary>
        public void Spawn(string networkID)
        {
            if (IsNetworkInitialized) return;
            if (NetworkManager.Instance.Client.NetworkGameClasses.ContainsKey(networkID))
            {
                Debug.LogError($"Network game class with id [{networkID}] already exists. Cannot spawn.");
                return;
            }
            Message message = Message.Create(MessageSendMode.Reliable,
                (ushort)NetworkMessageType.NetworkClassCreated);
            message.AddUInt(UpdateVersion);
            message.AddInt(GetRegisterID());
            message.AddString(networkID);
            message.AddString(ToJson());
            if (NetworkManager.Instance.Server.IsRunning && NetworkManager.Instance.Client.IsConnected)
            {
                OwnerID = NetworkManager.Instance.Client.ID;
                IsOwner = true;
                NetworkID = networkID;
                if (NetworkManager.Instance.Client.NetworkGameClasses.ContainsKey(NetworkID))
                {
                    Debug.LogError($"The NetworkID [{networkID}] already exists!");
                    return;
                }
                NetworkManager.Instance.Client.NetworkGameClasses.Add(NetworkID, this);
            }
            NetworkManager.Instance.Client.Send(message);
        }
        /// <summary>
        /// Called by the server when spawning over the network. Should not be called elsewhere.
        /// </summary>
        /// <param name="networkID"></param>
        /// <param name="ownerID"></param>
        /// <param name="json"></param>
        public void NetworkInitialize(string networkID, ushort ownerID)
        {
            if (IsNetworkInitialized) return;
            if (!(NetworkManager.Instance.Server.IsRunning && NetworkManager.Instance.Client.IsConnected))
            {
                NetworkID = networkID;
                OwnerID = ownerID;
                IsOwner = NetworkManager.Instance.Client.ID == ownerID;
            }
            Program.Game.RegisterGameClass(this);
            IsNetworkInitialized = true;
            OnNetworkInitialized();
        }
        /// <summary>
        /// Sets the current update version.
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
        /// Sends data to the server registered under this objects NetworkID.
        /// </summary>
        /// <param name="sendMode"></param>
        /// <param name="json"></param>
        /// <param name="type">The type of data being sent, in case of multiple update types.
        /// Leave default if only one update type is ever sent.</param>
        protected void Send(MessageSendMode sendMode, string json, ushort type = 0)
        {
            UpdateVersion++;
            if (!IsOwner)
            {
                Debug.LogError("Cannot send data using NetworkGameClass that does not belong to the client.");
                return;
            }
            Message message = Message.Create(sendMode, (ushort)NetworkMessageType.NetworkClassUpdated);
            message.AddString(NetworkID);
            message.AddUInt(UpdateVersion);
            message.AddUShort(type);
            message.AddString(json);
            NetworkManager.Instance.Client.Send(message);
        }
        /// <summary>
        /// Called by the server when this object should be disposed. Should not be called elsewhere.
        /// </summary>
        public void NetworkDispose()
        {
            Program.Game.UnregisterGameClass(this);
            GC.SuppressFinalize(this);
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
        /// Should check the received version against the stored one (<see cref="UpdateVersion"/>), if applicable.
        /// Can return true to ignore update versions.
        /// </summary>
        /// <param name="type">The type of update received.</param>
        /// <param name="version">The version number for the update received.</param>
        /// <returns></returns>
        public abstract bool CheckUpdateVersion(ushort type, uint version);
    }
}
