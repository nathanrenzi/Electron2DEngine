namespace Electron2D.Core.ECS
{
    /// <summary>
    /// Used to create a system for individual components to avoid CPU cache misses.
    /// https://matthall.codes/blog/ecs/
    /// </summary>
    /// <typeparam name="T">The type of component this system represents.</typeparam>
    public class BaseSystem<T> where T : Component
    {
        protected static List<T> components = new List<T>();

        public static void Register(T _component)
        {
            components.Add(_component);
        }

        public static void Start()
        {
            foreach (T component in components)
            {
                component.Start();
            }
        }

        public static void Update()
        {
            foreach (T component in components)
            {
                component.Update();
            }
        }
    }
}
