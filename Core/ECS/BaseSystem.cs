namespace Electron2D.Core.ECS
{
    /// <summary>
    /// Used to create a system for individual component types to avoid CPU cache misses.
    /// https://matthall.codes/blog/ecs/
    /// </summary>
    /// <typeparam name="T">The type of component this system represents.</typeparam>
    public class BaseSystem<T> where T : Component
    {
        protected static List<T> components = new List<T>();
        private static bool hasStarted = false;

        public static void Register(T _component)
        {
            components.Add(_component);
            if(hasStarted)
            {
                _component.Start();
            }
        }

        public static void Unregister(T _component)
        {
            components.Remove(_component);
        }

        public static void Start()
        {
            foreach (T component in components)
            {
                if (component != null && component.Enabled)
                {
                    component.Start();
                }
            }

            hasStarted = true;
        }

        public static void Update()
        {
            foreach (T component in components)
            {
                if(component != null && component.Enabled)
                {
                    component.Update();
                }
            }
        }

        public static void FixedUpdate()
        {
            try
            {
                foreach (T component in new List<T>(components))
                {
                    if (component != null && component.Enabled)
                    {
                        component.FixedUpdate();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public static List<T> GetComponents()
        {
            return components;
        }
    }
}
