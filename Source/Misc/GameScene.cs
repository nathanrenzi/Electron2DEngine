namespace Electron2D
{
    /// <summary>
    /// A scene of <see cref="IGameClass"/> objects that can be enabled, disabled, or disposed.
    /// </summary>
    public class GameScene : IGameClass
    {
        private List<IGameClass> _classes = new List<IGameClass>();
        protected bool _enabled = false;

        public GameScene()
        {
            Program.Game.RegisterGameClass(this);
            CreateGameClasses();
        }

        ~GameScene()
        {
            Dispose();
        }

        /// <summary>
        /// Creates game classes that should be added to the scene. Make sure to call <see cref="Register(IGameClass)"/>
        /// on every game class created.
        /// </summary>
        protected virtual void CreateGameClasses() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnDispose() { }

        public void Register(IGameClass gameClass)
        {
            if (gameClass == this) return;
            if (_classes.Contains(gameClass)) return;
            Program.Game.UnregisterGameClass(gameClass);
            _classes.Add(gameClass);
        }

        public void Unregister(IGameClass gameClass, bool registerToGameOnRemove = false)
        {
            if(_classes.Remove(gameClass) && registerToGameOnRemove)
            {
                Program.Game.RegisterGameClass(gameClass);
            }
        }

        /// <summary>
        /// Enables the scene, and all registered game classes.
        /// </summary>
        public void Enable()
        {
            _enabled = true;
        }

        /// <summary>
        /// Disables the scene, and all registered game classes.
        /// </summary>
        public void Disable()
        {
            _enabled = false;
        }

        public void Update()
        {
            if (!_enabled) return;
            for(int i = 0; i < _classes.Count; i++)
            {
                _classes[i].Update();
            }
            OnUpdate();
        }

        public void FixedUpdate()
        {
            if (!_enabled) return;
            for (int i = 0; i < _classes.Count; i++)
            {
                _classes[i].FixedUpdate();
            }
            OnFixedUpdate();
        }

        /// <summary>
        /// Disposes the scene, and all registered game objects. This will completely remove all game classes,
        /// unlike <see cref="Disable"/> which only prevents them from being rendered and updated.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Program.Game.UnregisterGameClass(this);
            for (int i = 0; i < _classes.Count; i++)
            {
                _classes[i].Dispose();
            }
            OnDispose();
            _classes = null;
        }
    }
}
