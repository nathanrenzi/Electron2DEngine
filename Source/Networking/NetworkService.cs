using Riptide;
using System.Reflection;

namespace Electron2D.Networking
{
    /// <summary>
    /// Marks a method as a handler for a server-side network message.
    /// The attributed method will be automatically invoked when a message with
    /// the specified <see cref="MessageID"/> is received while the server is
    /// running.
    /// </summary>
    /// <remarks>
    /// The target method must have the following signature:
    /// <list type="table">
    /// <item>
    /// <description><c>void Method(ushort clientID, Message message)</c></description>
    /// </item>
    /// </list>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ServerMessageAttribute : Attribute
    {
        public ushort MessageID { get; }
        public ServerMessageAttribute(ushort messageID) => MessageID = messageID;
    }

    /// <summary>
    /// Marks a method as a handler for a client-side network message.
    /// The attributed method will be automatically invoked when a message with
    /// the specified <see cref="MessageID"/> is received while the client is
    /// running.
    /// </summary>
    /// <remarks>
    /// The target method must have the following signature:
    /// <list type="table">
    /// <item>
    /// <description><c>void Method(Message message)</c></description>
    /// </item>
    /// </list>
    /// </remarks>
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

        private readonly Dictionary<ushort, Action<Message>> _clientHandlers = new();
        private readonly Dictionary<ushort, Action<ushort, Message>> _serverHandlers = new();

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
            var parameters = method.GetParameters();

            // Client handler
            if(parameters.Length == 1 && parameters[0].ParameterType == typeof(Message))
            {
                var del = (Action<Message>)Delegate.CreateDelegate(typeof(Action<Message>), this, method);
                _clientHandlers[messageID] = del;
                return;
            }

            // Server handler
            if (parameters.Length == 2 && parameters[0].ParameterType == typeof(ushort) &&
                parameters[1].ParameterType == typeof(Message))
            {
                var del = (Action<ushort, Message>)Delegate.CreateDelegate(typeof(Action<ushort, Message>), this, method);
                _serverHandlers[messageID] = del;
                return;
            }

            throw new InvalidOperationException($"Invalid handler signature for {method.Name}");
        }

        internal bool TryHandle(ushort messageID, Message message)
        {
            if (_clientHandlers.TryGetValue(messageID, out var handler))
            {
                handler(message);
                return true;
            }
            return false;
        }

        internal bool TryHandle(ushort messageID, ushort clientID, Message message)
        {
            if (_serverHandlers.TryGetValue(messageID, out var handler))
            {
                handler(clientID, message);
                return true;
            }
            return false;
        }

        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Dispose() { }
    }
}
