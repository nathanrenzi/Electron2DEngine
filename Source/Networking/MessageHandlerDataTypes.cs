namespace Electron2D.Networking
{
    public struct NetworkGameClassData
    {
        public uint Version;
        public int RegisterID;
        public string NetworkID;
        public ushort OwnerID;
        public string Json;
    }
    public struct NetworkGameClassUpdatedData
    {
        public string NetworkID;
        public uint Version;
        public ushort Type;
        public string Json;
    }

    public struct NetworkGameClassSyncSpawnData
    {
        public uint Version;
        public int RegisterID;
        public string NetworkID;
        public ushort ClientID;
        public string Json;
    }

    public struct NetworkGameClassRequestSyncData
    {
        public ushort ToClient;
        public NetworkGameClassData[] GameClasses;
    }
}
