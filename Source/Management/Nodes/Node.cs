using Electron2D.UserInterface;

namespace Electron2D
{
    /// <summary>
    /// The abstract base class for all scene hierarchy elements in Electron2D.
    /// Handles grouping, lifecycle management, and parent-child state propagation for 
    /// <see cref="IGameClass"/>, <see cref="UIComponent"/>, and nested <see cref="Node"/> instances.
    /// </summary>
    public abstract class Node : IGameClass
    {
        protected struct UIState
        {
            public bool Visible;
            public bool Interactable;
        }

        protected readonly List<IGameClass> _gameClasses = new List<IGameClass>();
        protected readonly List<UIComponent> _uiComponents = new List<UIComponent>();
        protected readonly Dictionary<UIComponent, UIState> _uiStateDictionary = new();

        protected bool _disposed = false;
        private bool _disabledByParent = false;
        private bool _desiredEnableState = true;
        private bool _hasParent = false;
        protected bool _enabled = true;

        public bool Enabled => _enabled && !_disabledByParent;

        public Node()
        {
            Engine.Game.RegisterGameClass(this);
            OnLoad();
        }

        ~Node()
        {
            Dispose(false);
        }

        /// <summary>
        /// Should create any objects that should be added to the node. Make sure to call <see cref="AddChild"/>
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
            return _gameClasses.Contains(gameClass);
        }

        public bool Contains(UIComponent uiComponent)
        {
            if (_disposed) return false;
            return _uiComponents.Contains(uiComponent);
        }

        private bool ContainsRecursive(Node node)
        {
            foreach (var cls in _gameClasses)
            {
                if (cls == node) return true;
                if (cls is Node sg && sg.ContainsRecursive(node)) return true;
            }
            return false;
        }

        public void AddChild(IGameClass gameClass)
        {
            if (_disposed || gameClass == this || _gameClasses.Contains(gameClass)) return;

            if (gameClass is Node sg)
            {
                if (sg._hasParent)
                {
                    Debug.LogError("Node already has a parent, cannot register.");
                    return;
                }

                if (sg.ContainsRecursive(this))
                {
                    throw new Exception("Circular dependency detected when trying to register Node.");
                }

                sg._hasParent = true;


                if (_enabled)
                {
                    sg._disabledByParent = false;
                    if (sg._desiredEnableState)
                    {
                        sg.EnableInternal(false);
                    }
                }
                else
                {
                    sg._disabledByParent = true;
                    sg.DisableInternal(true);
                }
            }

            Engine.Game.UnregisterGameClass(gameClass);
            _gameClasses.Add(gameClass);
        }

        public void AddChild(UIComponent uiComponent)
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

        public void RemoveChild(IGameClass gameClass)
        {
            if (_disposed) return;

            _gameClasses.Remove(gameClass);

            if (gameClass is Node sg)
            {
                sg._hasParent = false;

                // When removed, restore intended state if different from actual
                if (sg._desiredEnableState && !sg._enabled)
                {
                    sg.EnableInternal(false);
                }
                else if (sg._desiredEnableState && sg._enabled)
                {
                    sg.DisableInternal(false);
                }
            }
        }

        public void RemoveChild(UIComponent uiComponent)
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
        /// Enables the node, and all registered objects.
        /// </summary>
        public void Enable()
        {
            EnableInternal(false);
        }

        private void EnableInternal(bool parentEnabling)
        {
            if (_disposed) return;

            if (!parentEnabling)
            {
                _desiredEnableState = true;
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
            foreach (var cls in _gameClasses)
            {
                if (cls is Node sg)
                {
                    sg._disabledByParent = false;
                    if (sg._desiredEnableState)
                    {
                        sg.EnableInternal(false);
                    }
                }
            }

            OnEnable();
        }

        /// <summary>
        /// Disables the node, and all registered objects.
        /// </summary>
        public void Disable()
        {
            DisableInternal(false);
        }

        private void DisableInternal(bool parentDisabling)
        {
            if (_disposed) return;

            if (parentDisabling)
            {
                _disabledByParent = true;
            }
            else
            {
                _desiredEnableState = false;
            }

            if (!_enabled) return;
            _enabled = false;

            foreach (var uiComponent in _uiComponents)
            {
                RecordUIState(uiComponent);
                uiComponent.Visible = false;
                uiComponent.Interactable = false;
            }

            foreach (var cls in _gameClasses)
            {
                if (cls is Node sg)
                {
                    sg.DisableInternal(true);
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

            foreach (var gameClass in _gameClasses.ToArray())
            {
                gameClass.Update();
            }

            OnUpdate();
        }

        public void FixedUpdate()
        {
            if (_disposed || !_enabled) return;

            foreach (var gameClass in _gameClasses.ToArray())
            {
                gameClass.FixedUpdate();
            }

            OnFixedUpdate();
        }

        /// <summary>
        /// Disposes the node, and all registered objects.
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
                foreach (var gameClass in _gameClasses)
                {
                    gameClass.Dispose();
                }
                foreach (var uiComponent in _uiComponents)
                {
                    uiComponent.Dispose();
                }
                Engine.Game.UnregisterGameClass(this);
                OnDispose();
            }

            _gameClasses.Clear();
            _uiComponents.Clear();
            _uiStateDictionary.Clear();
        }
    }
}
