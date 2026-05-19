namespace Atlas2D.UI
{
    /// <summary>
    /// Controls how children are aligned within a <see cref="UILayout"/>.
    /// </summary>
    public enum UILayoutAlignment
    {
        /// <summary>
        /// Children are aligned to the start of the available space.
        /// </summary>
        Start,

        /// <summary>
        /// Children are centered in the available space.
        /// </summary>
        Center,

        /// <summary>
        /// Children are aligned to the end of the available space.
        /// </summary>
        End,

        /// <summary>
        /// Children are stretched to fill the available space.
        /// </summary>
        Stretch
    }
}
