namespace Electron2D.Patterns
{
    public class State<T>
    {
        public string Name { get; set; }
        public T ID { get; private set; }
        public bool CanSwitch { get; protected set; } = true;

        public State(T _id)
        {
            ID = _id;
        }
        public State(T _id, string _name) : this(_id)
        {
            Name = _name;
        }

        public delegate void DelegateNoArg();

        public DelegateNoArg OnEnter;
        public DelegateNoArg OnExit;
        public DelegateNoArg OnUpdate;

        public State(T _id,
            DelegateNoArg _onEnter,
            DelegateNoArg _onExit = null,
            DelegateNoArg _onUpdate = null) : this(_id)
        {
            OnEnter = _onEnter;
            OnExit = _onExit;
            OnUpdate = _onUpdate;
        }
        public State(T _id,
            string _name,
            DelegateNoArg _onEnter,
            DelegateNoArg _onExit = null,
            DelegateNoArg _onUpdate = null) : this(_id, _name)
        {
            OnEnter = _onEnter;
            OnExit = _onExit;
            OnUpdate = _onUpdate;
        }

        virtual public void Enter()
        {
            OnEnter?.Invoke();
        }

        virtual public void Exit()
        {
            OnExit?.Invoke();
        }
        virtual public void Update()
        {
            OnUpdate?.Invoke();
        }
    }
}