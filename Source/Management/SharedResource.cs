namespace Atlas2D
{
    public class SharedResource<T> where T : IDisposable
    {
        private class ControlBlock
        {
            public T Resource;
            public int RefCount;

            public ControlBlock(T resource)
            {
                Resource = resource;
                RefCount = 1;
            }
        }

        private ControlBlock? _block;

        private SharedResource(ControlBlock block)
        {
            _block = block;
        }

        public static SharedResource<T> Create(T resource)
        {
            return new SharedResource<T>(new ControlBlock(resource));
        }

        public SharedResource<T> AddRef()
        {
            if (_block == null) throw new InvalidOperationException("Handle is empty.");
            Interlocked.Increment(ref _block.RefCount);
            return new SharedResource<T>(_block);
        }

        public void Release()
        {
            if (_block == null) return;
            if (Interlocked.Decrement(ref _block.RefCount) == 0)
                _block.Resource.Dispose();
            _block = null;
        }

        public T Value
        {
            get
            {
                if (_block == null) throw new InvalidOperationException("Handle is empty.");
                return _block.Resource;
            }
        }

        public bool IsValid => _block != null;
        public int RefCount => _block?.RefCount ?? 0;
    }
}
