namespace Electron2D.Core.ECS
{
    public class Entity : IDisposable
    {
        private List<Component> components = new List<Component>();
        private bool _disposed;

        public void AddComponent(Component _component)
        {
            components.Add(_component);
            _component.Entity = this;
            _component.OnAdded();
        }

        public void RemoveComponent(Component _component)
        {
            components.Remove(_component);
            _component.Entity = null;
        }

        public T GetComponent<T>() where T : Component
        {
            foreach (Component component in components)
            {
                if (component.GetType().Equals(typeof(T)))
                {
                    return (T)component;
                }
            }
            return null;
        }

        ~Entity()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            OnDispose();

            for (int i = 0; i < components.Count; i++)
            {
                components[i].Dispose();
            }
            components.Clear();
        }
        protected virtual void OnDispose() { }
    }
}
