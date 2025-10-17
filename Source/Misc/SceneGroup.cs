using Electron2D.UserInterface;

namespace Electron2D
{
    /// <summary>
    /// A group of <see cref="IGameClass"/> and/or <see cref="UIComponent"/> objects that can be enabled, disabled, or disposed.
    /// </summary>
    public abstract class SceneGroup : IGameClass
    {
        private struct UIState
        {
            public bool Visible;
            public bool Interactable;
        }

        private List<IGameClass> _classes = new List<IGameClass>();
        private List<UIComponent> _uiComponents = new List<UIComponent>();
        private Dictionary<UIComponent, UIState> _uiStateDictionary = new();
        protected bool _enabled = false;
        protected bool _disposed = false;

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
            if (_disposed) return;
            if (gameClass == this) return;
            if (_classes.Contains(gameClass)) return;
            Engine.Game.UnregisterGameClass(gameClass);
            _classes.Add(gameClass);
        }

        public void Register(UIComponent uiComponent)
        {
            if (_disposed) return;
            if (_uiComponents.Contains(uiComponent)) return;
            if(!_enabled)
            {
                RecordUIState(uiComponent);
                uiComponent.Visible = false;
                uiComponent.Interactable = false;
            }
            _uiComponents.Add(uiComponent);
        }

        public void Unregister(IGameClass gameClass)
        {
            if (_disposed) return;
            _classes.Remove(gameClass);
        }

        public void Unregister(UIComponent uiComponent)
        {
            if (_disposed) return;
            if (_uiStateDictionary.ContainsKey(uiComponent))
            {
                UIState state = _uiStateDictionary[uiComponent];
                uiComponent.Visible = state.Visible;
                uiComponent.Interactable = state.Interactable;
                _uiStateDictionary.Remove(uiComponent);
            }
            _uiComponents.Remove(uiComponent);
        }

        /// <summary>
        /// Enables the group, and all registered objects.
        /// </summary>
        public void Enable()
        {
            if (_disposed) return;
            _enabled = true;
            foreach (var pair in _uiStateDictionary)
            {
                UIComponent uiComponent = pair.Key;
                uiComponent.Visible = pair.Value.Visible;
                uiComponent.Interactable = pair.Value.Interactable;
            }
            _uiStateDictionary.Clear();
            OnEnable();
        }

        /// <summary>
        /// Disables the group, and all registered objects.
        /// </summary>
        public void Disable()
        {
            if (_disposed) return;
            _enabled = false;
            _uiStateDictionary.Clear();
            for (int i = 0; i < _uiComponents.Count; i++)
            {
                RecordUIState(_uiComponents[i]);
            }
            OnDisable();
        }

        private void RecordUIState(UIComponent uiComponent)
        {
            _uiStateDictionary.Add(uiComponent, new UIState()
            {
                Visible = uiComponent.Visible,
                Interactable = uiComponent.Interactable
            });
            uiComponent.Visible = false;
            uiComponent.Interactable = false;
        }

        public void Update()
        {
            if (_disposed) return;
            if (!_enabled) return;
            for(int i = 0; i < _classes.Count; i++)
            {
                _classes[i].Update();
            }
            OnUpdate();
        }

        public void FixedUpdate()
        {
            if (_disposed) return;
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
            if (_disposed) return;
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
            _uiStateDictionary = null;
            _disposed = true;
        }
    }
}
