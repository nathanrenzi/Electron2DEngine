namespace Electron2D.Networking
{
    public enum NetworkMessageType
    {
        NetworkClassCreated = 60000,
        NetworkClassUpdated = 60001,
        NetworkClassDeleted = 60002,
        NetworkClassSync = 60003,
        NetworkClassRequestSyncData = 60004
    }
}
