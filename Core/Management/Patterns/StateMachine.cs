using Electron2D.Core.ECS;

namespace Electron2D.Core.Patterns
{
    public class StateMachine<T>
    {
        protected Dictionary<T, State<T>> states;
        protected State<T> currentState;
        protected State<T> previousState = null;

        public StateMachine()
        {
            states = new Dictionary<T, State<T>>();
        }

        public void Add(State<T> _state)
        {
            states.Add(_state.ID, _state);
        }

        public void Add(T _stateID, State<T> _state)
        {
            states.Add(_stateID, _state);
        }

        public State<T> GetState(T _stateID)
        {
            if (states.ContainsKey(_stateID))
                return states[_stateID];
            return null;
        }

        public void SetCurrentState(T _stateID)
        {
            State<T> state = states[_stateID];
            SetCurrentState(state);
        }

        public State<T> GetCurrentState()
        {
            return currentState;
        }

        public State<T> GetPreviousState()
        {
            return previousState;
        }

        public void SetCurrentState(State<T> _state)
        {
            if (currentState == _state || (currentState != null && !currentState.CanSwitch))
            {
                return;
            }

            if (currentState != null)
            {
                currentState.Exit();
            }

            previousState = currentState;
            currentState = _state;

            if (currentState != null)
            {
                currentState.Enter();
            }
        }

        public void Update()
        {
            if (currentState != null)
            {
                currentState.Update();
            }
        }
    }
}
