namespace Atlas2D.Networking
{
    internal enum BuiltInMessageType
    {
        NetworkNodeSpawned = 60000,
        NetworkNodeUpdated = 60001,
        NetworkNodeDespawned = 60002,
        NetworkNodeSync = 60003,
        NetworkNodeRequestSyncData = 60004,
    }
}
