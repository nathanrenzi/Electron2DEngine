namespace Electron2D.Core.Rendering
{
    public struct BufferUpdateItem
    {
        public int offset;
        public float[] data;

        public BufferUpdateItem(int _offset, float[] _data)
        {
            offset = _offset;
            data = _data;
        }
    }
}
