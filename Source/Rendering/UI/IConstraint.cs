namespace Electron2D.UI
{
    /// <summary>
    /// Defines a constraint that can be applied to a <see cref="UIElement"/> to modify its layout properties.
    /// </summary>
    public interface IConstraint
    {
        /// <summary>
        /// Applies this constraint to the given <see cref="UIElement"/>.
        /// </summary>
        /// <param name="element">The element to apply the constraint to.</param>
        void Apply(UIElement element);
    }
}
