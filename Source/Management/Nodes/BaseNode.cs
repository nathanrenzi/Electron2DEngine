namespace Electron2D
{
    /// <summary>
    /// A basic <see cref="Node"/> implementation that provides grouping and lifecycle management
    /// for game objects without requiring subclassing.
    /// </summary>
    public class BaseNode : Node
    {
        protected override void OnLoad() { }
        protected override void OnEnable() { }
        protected override void OnDisable() { }
        protected override void OnUpdate() { }
        protected override void OnFixedUpdate() { }
        protected override void OnDispose() { }
    }
}
