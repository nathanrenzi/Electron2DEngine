using Electron2D.UserInterface;

namespace Electron2D
{
    /// <summary>
    /// A scene of <see cref="IGameClass"/> and <see cref="UIComponent"/> objects that can be enabled, disabled, or disposed.
    /// </summary>
    public abstract class GameScene : IGameClass
    {
        private List<IGameClass> _classes = new List<IGameClass>();
        private List<UIComponent> _uiComponents = new List<UIComponent>();
        private bool[] _uiVisibilityState = null;
        protected bool _enabled = false;

        public GameScene()
        {
            Engine.Game.RegisterGameClass(this);
            CreateGameClasses();
        }

        ~GameScene()
        {
            Dispose();
        }

        /// <summary>
        /// Should create any game classes that should be added to the scene. Make sure to call <see cref="Register(IGameClass)"/>
        /// on every game class created.
        /// </summary>
        protected abstract void CreateGameClasses();
        protected abstract void OnUpdate();
        protected abstract void OnFixedUpdate();
        protected abstract void OnDispose();

        public void Register(IGameClass gameClass)
        {
            if (gameClass == this) return;
            if (_classes.Contains(gameClass)) return;
            Engine.Game.UnregisterGameClass(gameClass);
            _classes.Add(gameClass);
        }

        public void Register(UIComponent uiComponent)
        {
            if (_uiComponents.Contains(uiComponent)) return;
            _uiComponents.Add(uiComponent);
        }

        public void Unregister(IGameClass gameClass)
        {
            _classes.Remove(gameClass);
        }

        public void Unregister(UIComponent uiComponent)
        {
            _uiComponents.Remove(uiComponent);
        }

        /// <summary>
        /// Enables the scene, and all registered objects.
        /// </summary>
        public void Enable()
        {
            _enabled = true;
            if(_uiVisibilityState != null)
            {
                for (int i = 0; i < _uiComponents.Count; i++)
                {
                    _uiComponents[i].Visible = _uiVisibilityState[i];
                }
            }
        }

        /// <summary>
        /// Disables the scene, and all registered objects.
        /// </summary>
        public void Disable()
        {
            _enabled = false;
            _uiVisibilityState = new bool[_uiComponents.Count];
            for (int i = 0; i < _uiComponents.Count; i++)
            {
                _uiVisibilityState[i] = _uiComponents[i].Visible;
                _uiComponents[i].Visible = false;
            }
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
        /// Disposes the scene, and all registered objects.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Engine.Game.UnregisterGameClass(this);
            for (int i = 0; i < _classes.Count; i++)
            {
                _classes[i].Dispose();
            }
            for (int i = 0; i < _uiComponents.Count; i++)
            {
                _uiComponents[i].Dispose();
            }
            OnDispose();
            _classes = null;
            _uiComponents = null;
            _uiVisibilityState = null;
        }
    }
}
