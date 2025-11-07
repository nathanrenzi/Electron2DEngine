namespace Electron2D
{
    /// <summary>
    /// A <see cref="Node"/> that groups child nodes into named states and enables or disables them 
    /// automatically when the active state changes.
    /// </summary>
    public class StateNode : Node
    {
        private Dictionary<string, List<Node>> _states = new();
        private string _currentState = "";

        /// <summary>
        /// Adds a node to a given state. The node is also added as a child of this StateNode.
        /// </summary>
        /// <param name="state">The state that the node should be added to. Must not be <see langword="null"/> or empty.</param>
        /// <param name="node">The node to be added. Must not be <see langword="null"/>.</param>
        public void Add(string state, Node node)
        {
            if (!_states.TryGetValue(state, out var list))
            {
                _states[state] = list = new List<Node>();
            }

            if(list.Contains(node))
            {
                return;
            }

            list.Add(node);

            if(!_gameClasses.Contains(node))
            {
                AddChild(node);

                // Only enabling/disabling if the Node was not yet a child of this StateNode
                // to avoid overwriting the parental disabling.
                if(_currentState == state)
                {
                    node.Enable();
                }
                else
                {
                    node.Disable();
                }
            }
        }

        /// <summary>
        /// Removes the specified node from a state. If the node is no longer associated with any states after removing, it
        /// is also removed from being a child of this StateNode.
        /// </summary>
        /// <param name="state">The state that the node should be removed from. Must not be <see langword="null"/> or empty.</param>
        /// <param name="node">The node to be removed. Must not be <see langword="null"/>.</param>
        public void Remove(string state, Node node)
        {
            if(_states.ContainsKey(state))
            {
                _states[state].Remove(node);
            }

            bool found = false;
            foreach (var s in _states)
            {
                if(s.Value.Contains(node))
                {
                    found = true;
                    break;
                }
            }

            if(!found)
            {
                RemoveChild(node);
            }
        }

        /// <summary>
        /// Removes the specified node from all states, and also removes it as a child of this StateNode.
        /// </summary>
        /// <param name="node">The node to be removed. Must not be <see langword="null"/>.</param>
        public void Remove(Node node)
        {
            foreach (var list in _states.Values)
            {
                list.Remove(node);
            }
            RemoveChild(node);
        }

        /// <summary>
        /// Enables all nodes associated with the specified state, and disables all others.
        /// </summary>
        /// <param name="state">The name of the state whose nodes should be activated. Must not be <see langword="null"/> or empty.</param>
        public void SetState(string state)
        {
            _currentState = state;
            bool found = _states.ContainsKey(state);

            // First pass: disable all nodes not in the active state
            foreach (var (key, nodes) in _states)
            {
                if (key == state) continue;
                foreach (var node in nodes)
                {
                    node.Disable();
                }
            }

            // Second pass: enable all nodes in the active state
            if (found)
            {
                foreach (var node in _states[state])
                {
                    node.Enable();
                }
            }
        }

        protected override void OnDisable() { }
        protected override void OnDispose()
        {
            _states.Clear();
        }
        protected override void OnEnable()
        {
            SetState(_currentState);
        }
        protected override void OnFixedUpdate() { }
        protected override void OnLoad() { }
        protected override void OnUpdate() { }
    }
}
