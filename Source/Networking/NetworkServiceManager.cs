using Riptide;

namespace Electron2D.Networking
{
    internal class NetworkServiceManager
    {
        private readonly bool _isServer;
        private readonly List<NetworkService> _services = new();

        internal NetworkServiceManager(bool isServer)
        {
            _isServer = isServer;
        }

        internal void Register<T>() where T : NetworkService
        {
            T service = (T)Activator.CreateInstance(typeof(T), args: _isServer)!;
            _services.Add(service);
        }

        internal void Dispatch(ushort messageID, Message message)
        {
            foreach (var service in _services)
            {
                if (service.TryHandle(messageID, message))
                    return;
            }
        }
    }
}
