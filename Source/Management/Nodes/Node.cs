using Electron2D.UI;

namespace Electron2D
{
    /// <summary>
    /// The base class for all scene hierarchy elements in Electron2D.
    /// Handles grouping, lifecycle management, and parent-child state propagation for 
    /// <see cref="IGameClass"/>, <see cref="UIComponent"/>, and nested <see cref="Node"/> instances.
    /// </summary>
    public class Node : IGameClass
    {
        protected struct UIState
        {
            public bool Visible;
            public bool Interactable;
        }

        public Node? Parent { get; private set; } = null;
        protected readonly List<IGameClass> _gameClasses = new List<IGameClass>();
        protected readonly List<UIElement> _uiElements = new List<UIElement>();

        protected bool _disposed = false;
        private bool _disabledByParent = false;
        private bool _desiredEnableState = true;
        private bool _isLoaded = false;
        protected bool _enabled = true;

        public bool Enabled => _enabled && !_disabledByParent;

        public Node()
        {
            Engine.Game.RegisterGameClass(this);
            Load();
        }

        protected Node(bool load)
        {
            Engine.Game.RegisterGameClass(this);
            if(load)
            {
                Load();
            }
        }

        protected void Load()
        {
            if (_isLoaded) return;
            _isLoaded = true;
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
        protected virtual void OnLoad() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnDispose() { }
        protected virtual void OnParentChanged(Node newParent) { }

        public bool Contains(IGameClass gameClass)
        {
            if (_disposed) return false;
            return _gameClasses.Contains(gameClass);
        }

        public bool Contains(UIElement element)
        {
            if (_disposed) return false;
            return _uiElements.Contains(element);
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
                if (sg.Parent != null)
                {
                    Debug.LogError("Node already has a parent, cannot register.");
                    return;
                }

                if (sg.ContainsRecursive(this))
                {
                    throw new Exception("Circular dependency detected when trying to register Node.");
                }

                sg.SetParent(this);


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

        public void AddChild(UIElement element)
        {
            if (_disposed || _uiElements.Contains(element)) return;
            element.Enabled = _enabled;
            _uiElements.Add(element);
        }

        public void RemoveChild(IGameClass gameClass)
        {
            if (_disposed) return;

            _gameClasses.Remove(gameClass);

            if (gameClass is Node sg)
            {
                sg.SetParent(null);

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

        public void RemoveChild(UIElement element)
        {
            if (_disposed) return;
            element.Enabled = true;
            _uiElements.Remove(element);
        }

        public void SetParent(Node parent)
        {
            if (_disposed) return;

            Parent = parent;
            OnParentChanged(parent);
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

            // Enable children
            foreach (var element in _uiElements)
            {
                element.Enabled = true;
            }
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

            foreach (var element in _uiElements)
            {
                element.Enabled = false;
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
                foreach (var element in _uiElements)
                {
                    element.Dispose();
                }
                Engine.Game.UnregisterGameClass(this);
                OnDispose();
            }

            _gameClasses.Clear();
            _uiElements.Clear();
        }
    }
}
