using Riptide;
using System.Reflection;

namespace Electron2D.Networking
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ServerMessageAttribute : Attribute
    {
        public ushort MessageID { get; }
        public ServerMessageAttribute(ushort messageID) => MessageID = messageID;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ClientMessageAttribute : Attribute
    {
        public ushort MessageID { get; }
        public ClientMessageAttribute(ushort messageID) => MessageID = messageID;
    }

    /// <summary>
    /// Handles network messages. Use <see cref="ServerMessageAttribute"/> and <see cref="ClientMessageAttribute"/> to handle received messages.
    /// </summary>
    public abstract class NetworkService : IGameClass
    {
        protected bool IsServer { get; }

        private readonly Dictionary<ushort, Action<Message>> _handlers = new();

        protected NetworkService(bool isServer)
        {
            IsServer = isServer;
            AutoRegister();
            Engine.Game.RegisterGameClass(this);
        }

        private void AutoRegister()
        {
            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var method in methods)
            {
                foreach (var attr in method.GetCustomAttributes())
                {
                    if(attr is ServerMessageAttribute s && IsServer)
                    {
                        Bind(method, s.MessageID);
                    }

                    if (attr is ClientMessageAttribute c && !IsServer)
                    {
                        Bind(method, c.MessageID);
                    }
                }
            }
        }

        private void Bind(MethodInfo method, ushort messageID)
        {
            var del = (Action<Message>) Delegate.CreateDelegate(typeof(Action<Message>), this, method);
            _handlers[messageID] = del;
        }

        internal bool TryHandle(ushort messageID, Message message)
        {
            if (_handlers.TryGetValue(messageID, out var handler))
            {
                handler(message);
                return true;
            }
            return false;
        }

        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Dispose() { }
    }
}
