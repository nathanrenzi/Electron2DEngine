namespace Electron2D.Core.ECS
{
    public class Component
    {
        public Entity Entity;

        public virtual void Start() { }
        public virtual void Update() { }
    }
}
