using Riptide;

namespace Electron2D.Networking
{
    /// <summary>
    /// An abstract class that provides the boilerplate for networked game classes.
    /// Abstract method <see cref="ReceiveData(ushort, string)"/> receives update data
    /// from the server and interprets it. <see cref="SendData(MessageSendMode, string, ushort)"/>
    /// sends update data to the server, which is then received and interpreted by all other clients.
    /// <see cref="ToJson()"/> is called by a message from the server, requesting the current
    /// state of the game class so that a connecting client can properly initialize their object. Any
    /// subclasses of <see cref="NetworkGameClass"/> must call <see cref="Networking.RegisterNetworkGameClass"/>
    /// in the <see cref="Game.Initialize"/> method to be properly instantiated over the network.
    /// </summary>
    public abstract class NetworkGameClass : IGameClass
    {
        public ushort NetworkID { get; private set; } = ushort.MaxValue;
        public ushort OwnerID { get; private set; } = ushort.MaxValue;
        public bool IsOwner { get; private set; } = false;
        public bool IsNetworkInitialized { get; private set; }

        /// <summary>
        /// Sends a request to the server to spawn this object.
        /// </summary>
        public void Spawn()
        {
            if (IsNetworkInitialized) return;
            Message message = Message.Create(MessageSendMode.Reliable,
                (ushort)Networking.NetworkingMessageType.NetworkClassCreated);
            message.AddInt(GetRegisterID());
            message.AddString(ToJson());
            if(Networking.Instance.IsHost)
            {
                OwnerID = Networking.Instance.Client.Id;
                IsOwner = true;
                NetworkID = Networking.Instance.GetNextNetworkID();
                Networking.Instance.ClientNetworkGameClasses.Add(NetworkID, this);
            }
            Networking.Instance.Client.Send(message);
        }

        /// <summary>
        /// Called by the server when spawning over the network. Should not be called elsewhere.
        /// </summary>
        /// <param name="networkID"></param>
        /// <param name="ownerID"></param>
        /// <param name="json"></param>
        public void ServerSpawn(ushort networkID, ushort ownerID, string json)
        {
            if(IsNetworkInitialized) return;
            if(!Networking.Instance.IsHost)
            {
                NetworkID = networkID;
                OwnerID = ownerID;
                IsOwner = Networking.Instance.Client.Id == ownerID;
                FromJson(json);
            }
            Program.Game.RegisterGameClass(this);
            IsNetworkInitialized = true;
            OnNetworkInitialized();
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
                    (ushort)Networking.NetworkingMessageType.NetworkClassDeleted);
                Networking.Instance.Client.Send(message);
            }
            Program.Game.UnregisterGameClass(this);
            GC.SuppressFinalize(this);
        }

        public abstract void FixedUpdate();
        public abstract void Update();

        /// <summary>
        /// Should initialize the network object from json data. Json should contain
        /// all data, including any data that has been changed since the object has been created
        /// so that players joining after object has been created are up to date.
        /// </summary>
        /// <param name="json">The json that is used to create the network class.</param>
        public abstract void FromJson(string json);
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
        /// Sends the update data to the server.
        /// </summary>
        /// <param name="sendMode"></param>
        /// <param name="json"></param>
        /// <param name="type">The type of data being sent, in case of multiple update types.
        /// Leave default if only one update type is ever sent.</param>
        protected void SendData(MessageSendMode sendMode, string json, ushort type = 0)
        {
            Message message = Message.Create(sendMode, Networking.NetworkingMessageType.NetworkClassUpdated);
            message.AddUShort(NetworkID);
            message.AddUShort(type);
            message.AddString(json);
        }
        /// <summary>
        /// Returns the register ID of the class. The register ID must be set by passing the value returned
        /// from <see cref="Networking.RegisterNetworkGameClass"/> into a static method in each subclass.
        /// </summary>
        /// <returns></returns>
        public abstract int GetRegisterID();
        /// <summary>
        /// Called when the server initializes the network game class.
        /// </summary>
        public abstract void OnNetworkInitialized();
    }
}
