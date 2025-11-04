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

        protected bool _disposed = false;
        private bool _disabledByParent = false;
        private bool _intendedEnableState = false;
        private bool _hasParent = false;
        protected bool _enabled = false;

        public bool Enabled => _enabled && !_disabledByParent;

        public SceneGroup()
        {
            Engine.Game.RegisterGameClass(this);
            OnLoad();
        }

        ~SceneGroup()
        {
            Dispose(false);
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

        public bool Contains(IGameClass gameClass)
        {
            if (_disposed) return false;
            return _classes.Contains(gameClass);
        }

        public bool Contains(UIComponent uiComponent)
        {
            if (_disposed) return false;
            return _uiComponents.Contains(uiComponent);
        }

        private bool ContainsRecursive(SceneGroup sceneGroup)
        {
            foreach (var cls in _classes)
            {
                if (cls == sceneGroup) return true;
                if (cls is SceneGroup sg && sg.ContainsRecursive(sceneGroup)) return true;
            }
            return false;
        }

        public void Register(IGameClass gameClass)
        {
            if (_disposed || gameClass == this || _classes.Contains(gameClass)) return;

            if (gameClass is SceneGroup sg)
            {
                if (sg._hasParent)
                {
                    Debug.LogError("SceneGroup already has a parent, cannot register.");
                    return;
                }

                if (sg.ContainsRecursive(this))
                {
                    throw new Exception("Circular dependency detected when trying to register SceneGroup.");
                }

                sg._hasParent = true;


                if (_enabled)
                {
                    sg._disabledByParent = false;
                    if (sg._intendedEnableState)
                    {
                        sg.Enable();
                    }
                }
                else
                {
                    sg._disabledByParent = true;
                    sg.Disable(true);
                }
            }

            Engine.Game.UnregisterGameClass(gameClass);
            _classes.Add(gameClass);
        }

        public void Register(UIComponent uiComponent)
        {
            if (_disposed || _uiComponents.Contains(uiComponent)) return;

            if (!_enabled)
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

            if (gameClass is SceneGroup sg)
            {
                sg._hasParent = false;

                // When removed, restore intended state if different from actual
                if (sg._intendedEnableState && !sg._enabled)
                {
                    sg.Enable();
                }
                else if (sg._intendedEnableState && sg._enabled)
                {
                    sg.Disable();
                }
            }
        }

        public void Unregister(UIComponent uiComponent)
        {
            if (_disposed) return;

            if (_uiStateDictionary.TryGetValue(uiComponent, out var state))
            {
                uiComponent.Visible = state.Visible;
                uiComponent.Interactable = state.Interactable;
                _uiStateDictionary.Remove(uiComponent);
            }

            _uiComponents.Remove(uiComponent);
        }

        /// <summary>
        /// Enables the group, and all registered objects.
        /// </summary>
        public void Enable(bool parentEnabling = false)
        {
            if (_disposed) return;

            if (!parentEnabling)
            {
                _intendedEnableState = true;
            }

            if (_enabled || _disabledByParent) return;
            _enabled = true;

            // Restore UI
            foreach (var pair in _uiStateDictionary)
            {
                UIComponent uiComponent = pair.Key;
                uiComponent.Visible = pair.Value.Visible;
                uiComponent.Interactable = pair.Value.Interactable;
            }
            _uiStateDictionary.Clear();

            // Enable children
            foreach (var cls in _classes)
            {
                if (cls is SceneGroup sg)
                {
                    sg._disabledByParent = false;
                    if (sg._intendedEnableState)
                    {
                        sg.Enable();
                    }
                }
            }

            OnEnable();
        }

        /// <summary>
        /// Disables the group, and all registered objects.
        /// </summary>
        public void Disable(bool parentDisabling = false)
        {
            if (_disposed) return;

            if (parentDisabling)
            {
                _disabledByParent = true;
            }
            else
            {
                _intendedEnableState = false;
            }

            // Preventing state from being recorded again
            if (!_enabled) return;
            _enabled = false;

            foreach (var uiComponent in _uiComponents)
            {
                RecordUIState(uiComponent);
                uiComponent.Visible = false;
                uiComponent.Interactable = false;
            }

            foreach (var cls in _classes)
            {
                if (cls is SceneGroup sg)
                {
                    sg.Disable(true);
                }
            }

            OnDisable();
        }

        private void RecordUIState(UIComponent uiComponent)
        {
            _uiStateDictionary[uiComponent] = new UIState()
            {
                Visible = uiComponent.Visible,
                Interactable = uiComponent.Interactable
            };
        }

        public void Update()
        {
            if (_disposed || !_enabled) return;

            foreach (var gameClass in _classes.ToArray())
            {
                gameClass.Update();
            }

            OnUpdate();
        }

        public void FixedUpdate()
        {
            if (_disposed || !_enabled) return;

            foreach (var gameClass in _classes.ToArray())
            {
                gameClass.FixedUpdate();
            }

            OnFixedUpdate();
        }

        /// <summary>
        /// Disposes the group, and all registered objects.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                Engine.Game.UnregisterGameClass(this);
                foreach (var gameClass in _classes)
                {
                    gameClass.Dispose();
                }
                foreach (var uiComponent in _uiComponents)
                {
                    uiComponent.Dispose();
                }
                OnDispose();
            }

            _classes.Clear();
            _uiComponents.Clear();
            _uiStateDictionary.Clear();
        }
    }
}
