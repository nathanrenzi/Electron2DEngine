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

        public void Register<T>() where T : NetworkService
        {
            T service = (T)Activator.CreateInstance(typeof(T), args: _isServer)!;
            _services.Add(typeof(T), service);
        }

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
