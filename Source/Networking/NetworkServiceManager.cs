using Riptide;

namespace Electron2D.Networking
{
    public sealed class NetworkServiceManager
    {
        private readonly bool _isServer;
        private readonly Dictionary<Type, NetworkService> _services = new();

        internal NetworkServiceManager(bool isServer)
        {
            _isServer = isServer;
        }

        public void Register(Type type)
        {
            var service = (NetworkService)Activator.CreateInstance(type, args: _isServer)!;
            _services.Add(type, service);
        }

        public bool HasService<T>() where T : NetworkService => _services.ContainsKey(typeof(T));

        public T Get<T>() where T : NetworkService
        {
            return (T)_services[typeof(T)];
        }

        internal void Dispatch(ushort messageID, Message message)
        {
            foreach (var service in _services)
            {
                if (service.Value.TryHandle(messageID, message))
                    return;
            }
        }

        internal void Dispatch(ushort messageID, ushort clientID, Message message)
        {
            foreach (var service in _services)
            {
                if (service.Value.TryHandle(messageID, clientID, message))
                    return;
            }
        }
    }
}
