using Riptide;

namespace Electron2D.Networking
{
    internal struct NetworkGameClassData
    {
        public uint Version;
        public int RegisterID;
        public string NetworkID;
        public ushort OwnerID;
        public string Json;
    }
    internal struct NetworkGameClassUpdatedData
    {
        public MessageSendMode MessageSendMode;
        public string NetworkID;
        public uint Version;
        public ushort Type;
        public string Json;
    }

    internal struct NetworkGameClassSyncSpawnData
    {
        public uint Version;
        public int RegisterID;
        public string NetworkID;
        public ushort ClientID;
        public string Json;
    }

    internal struct NetworkGameClassRequestSyncData
    {
        public ushort ToClient;
        public NetworkGameClassData[] GameClasses;
    }
}
