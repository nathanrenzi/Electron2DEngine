namespace Electron2D.Rendering.PostProcessing
{
    public class PostProcessingStack
    {
        public int Priority { get; private set; }
        public int Size
        {
            get
            {
                return _stack.Count;
            }
        }

        private List<IPostProcess> _stack = new List<IPostProcess>();

        public PostProcessingStack(int priority)
        {
            Priority = priority;
        }

        public IPostProcess Get(int index)
        {
            return _stack[index];
        }

        public void Add(IPostProcess effect)
        {
            _stack.Add(effect);
        }

        public void AddFirst(IPostProcess effect)
        {
            _stack.Insert(0, effect);
        }

        public void RemoveAll()
        {
            _stack.Clear();
        }
    }
}