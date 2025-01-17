namespace Electron2D.ECS
{
    public class Entity : IDisposable
    {
        public static List<Entity> ActiveEntities { get; private set; } = new List<Entity>();

        private List<Component> components = new List<Component>();
        private bool _disposed;

        public Entity()
        {
            ActiveEntities.Add(this);
        }

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

        public T[] GetComponentsOfType<T>() where T : Component
        {
            List<T> c = new List<T>();
            foreach (Component component in components)
            {
                if (component.GetType().Equals(typeof(T)))
                {
                    c.Add((T)component);
                }
            }
            return c.ToArray();
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

            ActiveEntities.Remove(this);

            for (int i = 0; i < components.Count; i++)
            {
                components[i].Dispose();
            }
            components.Clear();
        }
        protected virtual void OnDispose() { }
    }
}
