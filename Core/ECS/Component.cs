namespace Electron2D.Core.ECS
{
    public class Component : IDisposable
    {
        public Entity Entity;
        public bool Enabled = true;
        private bool _disposed;

        public virtual void OnComponentAdded() { }
        public virtual void Start() { }
        public virtual void Update() { }

        ~Component()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            OnDispose();
            if (Entity != null)
            {
                Entity.RemoveComponent(this);
            }
        }
        protected virtual void OnDispose() { }

        public T GetComponent<T>() where T : Component
        {
            return Entity.GetComponent<T>();
        }
    }
}
