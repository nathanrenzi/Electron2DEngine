using Electron2D.UserInterface;

namespace Electron2D
{
    /// <summary>
    /// A group of <see cref="IGameClass"/> and/or <see cref="UIComponent"/> objects that can be enabled, disabled, or disposed.
    /// </summary>
    public abstract class SceneGroup : IGameClass
    {
        private List<IGameClass> _classes = new List<IGameClass>();
        private List<UIComponent> _uiComponents = new List<UIComponent>();
        private bool[] _uiVisibilityState = null;
        protected bool _enabled = false;

        public SceneGroup()
        {
            Engine.Game.RegisterGameClass(this);
            OnLoad();
        }

        ~SceneGroup()
        {
            Dispose();
        }

        /// <summary>
        /// Should create any objects that should be added to the group. Make sure to call <see cref="Register"/>
        /// on every <see cref="IGameClass"/> or <see cref="UIComponent"/> created.
        /// </summary>
        protected abstract void OnLoad();
        protected abstract void OnEnable();
        protected abstract void OnDisable();
        protected abstract void OnUpdate();
        protected abstract void OnFixedUpdate();
        protected abstract void OnDispose();

        public void Register(IGameClass gameClass)
        {
            if (_classes == null || _uiComponents == null) return;
            if (gameClass == this) return;
            if (_classes.Contains(gameClass)) return;
            Engine.Game.UnregisterGameClass(gameClass);
            _classes.Add(gameClass);
        }

        public void Register(UIComponent uiComponent)
        {
            if (_classes == null || _uiComponents == null) return;
            if (_uiComponents.Contains(uiComponent)) return;
            _uiComponents.Add(uiComponent);
        }

        public void Unregister(IGameClass gameClass)
        {
            if (_classes == null || _uiComponents == null) return;
            _classes.Remove(gameClass);
        }

        public void Unregister(UIComponent uiComponent)
        {
            if (_classes == null || _uiComponents == null) return;
            _uiComponents.Remove(uiComponent);
        }

        /// <summary>
        /// Enables the group, and all registered objects.
        /// </summary>
        public void Enable()
        {
            if (_classes == null || _uiComponents == null) return;
            _enabled = true;
            if(_uiVisibilityState != null)
            {
                for (int i = 0; i < _uiComponents.Count; i++)
                {
                    _uiComponents[i].Visible = _uiVisibilityState[i];
                }
            }
            OnEnable();
        }

        /// <summary>
        /// Disables the group, and all registered objects.
        /// </summary>
        public void Disable()
        {
            if (_classes == null || _uiComponents == null) return;
            _enabled = false;
            _uiVisibilityState = new bool[_uiComponents.Count];
            for (int i = 0; i < _uiComponents.Count; i++)
            {
                _uiVisibilityState[i] = _uiComponents[i].Visible;
                _uiComponents[i].Visible = false;
            }
            OnDisable();
        }

        public void Update()
        {
            if (_classes == null || _uiComponents == null) return;
            if (!_enabled) return;
            for(int i = 0; i < _classes.Count; i++)
            {
                _classes[i].Update();
            }
            OnUpdate();
        }

        public void FixedUpdate()
        {
            if (_classes == null || _uiComponents == null) return;
            if (!_enabled) return;
            for (int i = 0; i < _classes.Count; i++)
            {
                _classes[i].FixedUpdate();
            }
            OnFixedUpdate();
        }

        /// <summary>
        /// Disposes the group, and all registered objects.
        /// </summary>
        public void Dispose()
        {
            if (_classes == null || _uiComponents == null) return;
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
