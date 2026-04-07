namespace Electron2D.UI
{
    public enum ScrollContentSizing
    {
        /// <summary>Content sizes to its children naturally, no viewport constraint.</summary>
        None,
        /// <summary>Content is clamped to at least the viewport size on this axis.</summary>
        Min,
        /// <summary>Content matches the viewport size exactly on this axis.</summary>
        Match
    }
}
