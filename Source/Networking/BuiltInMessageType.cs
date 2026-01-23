namespace Electron2D.Networking
{
    internal enum BuiltInMessageType
    {
        NetworkClassSpawned = 60000,
        NetworkClassUpdated = 60001,
        NetworkClassDespawned = 60002,
        NetworkClassSync = 60003,
        NetworkClassRequestSyncData = 60004,
    }
}
