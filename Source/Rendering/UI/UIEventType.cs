namespace Atlas2D.UI
{
    /// <summary>
    /// Defines the types of events that can be raised on a <see cref="UIElement"/>.
    /// </summary>
    public enum UIEventType
    {
        /// <summary>A mouse button was pressed over the element.</summary>
        MouseDown,

        /// <summary>A mouse button was released over the element.</summary>
        MouseUp,

        /// <summary>The mouse cursor entered the element's bounds.</summary>
        MouseEnter,

        /// <summary>The mouse cursor left the element's bounds.</summary>
        MouseLeave,

        /// <summary>The mouse scroll wheel was used over the element.</summary>
        MouseScroll,

        /// <summary>A mouse button was pressed and released over the element.</summary>
        Click,

        /// <summary>A drag operation began on the element.</summary>
        DragStart,

        /// <summary>The element is being dragged.</summary>
        Drag,

        /// <summary>A drag operation ended on the element.</summary>
        DragEnd,

        /// <summary>The element gained input focus.</summary>
        GainFocus,

        /// <summary>The element lost input focus.</summary>
        LoseFocus,

        /// <summary>The element became visible.</summary>
        GainVisibility,

        /// <summary>The element became hidden.</summary>
        LoseVisibility,

        /// <summary>The element became interactable.</summary>
        GainInteractability,

        /// <summary>The element became non-interactable.</summary>
        LoseInteractability
    }
}
