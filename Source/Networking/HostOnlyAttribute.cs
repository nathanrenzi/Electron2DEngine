namespace Atlas2D.Networking
{
    /// <summary>
    /// Marks a <see cref="NetworkNode"/> subclass as host-only. Non-host clients are blocked
    /// from spawning the node both on the client (before any message is sent) and on the server.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class HostOnlyAttribute : Attribute { }
}
