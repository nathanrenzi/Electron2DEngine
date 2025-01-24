namespace Electron2D
{
    public interface IGameClass : IDisposable
    {
        public void Update();
        public void FixedUpdate();
    }
}
