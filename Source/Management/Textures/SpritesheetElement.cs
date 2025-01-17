namespace Electron2D
{
    /// <summary>
    /// This struct represents a spritesheet on one of the texture slots in the GPU
    /// </summary>
    public struct SpritesheetElement
    {
        public readonly int totalSpriteColumns;
        public readonly int totalSpriteRows;

        public SpritesheetElement(int _totalSpriteColumns, int _totalSpriteRows)
        {
            totalSpriteColumns = _totalSpriteColumns;
            totalSpriteRows = _totalSpriteRows;
        }
    }
}
