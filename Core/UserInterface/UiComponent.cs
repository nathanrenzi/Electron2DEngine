using System.Numerics;
using Electron2D.Core.Rendering;

namespace Electron2D.Core.UI
{
    public abstract class UiComponent
    {
        public bool visible = true;
        public float position;
        public float sizeX;
        public float sizeY;
        public Vector2 anchor;
        public IRenderer renderer;
        public int uiRenderLayer = 0;
        public List<UiListener> listeners { get; private set; } = new List<UiListener>();

        public UiComponent(IRenderer _customRenderer = null)
        {
            if (_customRenderer != null)
            {
                renderer = _customRenderer;
            }
            else
            {
                // Set the renderer to be a new advanced sprite / ui renderer that allows for tiling
                // Also set the shader of this renderer to UiShader, a shader that will allow for rounded corners
                //  and other common ui functionality built-in
            }
        }

        public virtual bool CheckBounds(Vector2 _position)
        {
            float leftXBound = sizeX * (anchor.X * sizeX);
            float rightXBound = sizeX * (-anchor.X * sizeX);
            float topYBound = sizeY * (-anchor.Y * sizeY);
            float bottomYBound = sizeY * (anchor.Y * sizeY);

            if(_position.X >= leftXBound && _position.X <= rightXBound
                && _position.Y >= bottomYBound && _position.Y <= topYBound)
            {
                // The point is within the bounds
                return true;
            }

            return false;
        }

        public void AddUiListener(UiListener _listener)
        {
            listeners.Add(_listener);
        }

        public void RemoveUiListener(UiListener _listener)
        {
            listeners.Remove(_listener);
        }

        public void InvokeUiAction(UiEvent _event)
        {
            for (int i = 0; i < listeners.Count; i++)
            {
                listeners[i].OnUiAction(this, _event);
            }
        }

        public virtual void Render()
        {
            renderer.Render();
        }
    }

    public interface UiListener
    {
        public void OnUiAction(object _sender, UiEvent _event);
    }

    public enum UiEvent
    {
        Click,
        ClickDown,
        ClickUp,
        LeftClick,
        LeftClickDown,
        LeftClickUp,
        RightClick,
        RightClickDown,
        RightClickUp,
        Hover,
        HoverStart,
        HoverEnd
    }
}
