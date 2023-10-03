namespace Electron2D.Core.ECS
{
    public class Entity
    {
        public int ID { get; set; }

        private List<Component> components = new List<Component>();

        public void AddComponent(Component _component)
        {
            components.Add(_component);
            _component.Entity = this;
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
    }
}
