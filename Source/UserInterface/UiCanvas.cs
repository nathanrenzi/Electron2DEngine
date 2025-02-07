using System.Numerics;

namespace Electron2D.UserInterface
{
    public class UICanvas
    {
        public class UIFrameTickData
        {
            public bool IsHovered;
            public bool IsLeftClicked;
            public bool IsMiddleClicked;
            public bool IsRightClicked;
            public bool IsClicked;
            public bool IsInteractable;
        }

        private List<UIComponent> _activeComponents = new List<UIComponent>();
        private UIComponent _focusedComponent = null;

        // Add an ActiveUiComponent class that also holds a reference to the constraints being used
        //  and use that for the above list instead

        private bool _initialized = false;

        public void Initialize()
        {
            if (_initialized == true) return;
            _initialized = true;

            Game.LateUpdateEvent += CheckForInput;
        }

        public void RegisterUiComponent(UIComponent component)
        {
            if (_activeComponents.Contains(component)) return;

            component.SetParentCanvas(this);
            _activeComponents.Add(component);
        }

        public void UnregisterUiComponent(UIComponent component)
        {
            if (!_activeComponents.Contains(component)) return;

            _activeComponents.Remove(component);
        }

        public void CheckForInput()
        {
            Vector2 mousePos = Input.GetMouseWorldPosition();
            Vector2 mousePosScreen = Input.GetOffsetMousePositionScaled(true);
            int hoveredCount = 0;
            for (int i = 0; i < _activeComponents.Count; i++)
            {
                UIComponent component = _activeComponents[i];
                UIFrameTickData lastFrame = component.LastFrameData;
                UIFrameTickData thisFrame = component.ThisFrameData;

                // Checking if UI Component has just disabled interactability
                thisFrame.IsInteractable = component.Interactable;
                if (lastFrame.IsInteractable == true && thisFrame.IsInteractable == false)
                {
                    if (lastFrame.IsHovered)
                    {
                        thisFrame.IsHovered = false;
                        component.InvokeUIEvent(UIEvent.HoverEnd);
                    }
                    if (_focusedComponent == component)
                    {
                        component.InvokeUIEvent(UIEvent.LoseFocus);
                        _focusedComponent = null;
                    }
                    component.InvokeUIEvent(UIEvent.InteractabilityEnd);
                }
                if (!component.Interactable)
                {
                    thisFrame.IsClicked = false;
                    lastFrame.IsHovered = false;
                    lastFrame.IsLeftClicked = false;
                    lastFrame.IsMiddleClicked = false;
                    lastFrame.IsRightClicked = false;
                    lastFrame.IsClicked = false;
                    lastFrame.IsInteractable = false;
                    continue;
                }

                thisFrame.IsHovered = component.CheckBounds(component.UseScreenPosition ? mousePosScreen : mousePos);
                if (thisFrame.IsHovered)
                {
                    hoveredCount++;
                    // Mouse is hovering over UI component
                    component.InvokeUIEvent(UIEvent.Hover);

                    if (thisFrame.IsHovered == true && lastFrame.IsHovered == false)
                    {
                        component.InvokeUIEvent(UIEvent.HoverStart);
                    }

                    // Left click
                    if (Input.GetMouseButtonDown(MouseButton.Left))
                    {
                        thisFrame.IsLeftClicked = true;
                        component.InvokeUIEvent(UIEvent.LeftClickDown);
                        component.InvokeUIEvent(UIEvent.ClickDown);
                        if(_focusedComponent != component)
                        {
                            _focusedComponent?.InvokeUIEvent(UIEvent.LoseFocus);
                            _focusedComponent = component;
                            component.InvokeUIEvent(UIEvent.Focus);
                        }
                    }

                    if (Input.GetMouseButton(MouseButton.Left))
                    {
                        component.InvokeUIEvent(UIEvent.LeftClick);
                        component.InvokeUIEvent(UIEvent.Click);
                    }
                    // ----------------------

                    // Middle click
                    if (Input.GetMouseButtonDown(MouseButton.Middle))
                    {
                        thisFrame.IsMiddleClicked = true;
                        component.InvokeUIEvent(UIEvent.MiddleClickDown);
                        component.InvokeUIEvent(UIEvent.ClickDown);
                        if (_focusedComponent != component)
                        {
                            _focusedComponent?.InvokeUIEvent(UIEvent.LoseFocus);
                            _focusedComponent = component;
                            component.InvokeUIEvent(UIEvent.Focus);
                        }
                    }

                    if (Input.GetMouseButton(MouseButton.Middle))
                    {
                        component.InvokeUIEvent(UIEvent.MiddleClick);
                        component.InvokeUIEvent(UIEvent.Click);
                    }
                    // ----------------------

                    // Right click
                    if (Input.GetMouseButtonDown(MouseButton.Right))
                    {
                        thisFrame.IsRightClicked = true;
                        component.InvokeUIEvent(UIEvent.RightClickDown);
                        component.InvokeUIEvent(UIEvent.ClickDown);
                        if (_focusedComponent != component)
                        {
                            _focusedComponent?.InvokeUIEvent(UIEvent.LoseFocus);
                            _focusedComponent = component;
                            component.InvokeUIEvent(UIEvent.Focus);
                        }
                    }

                    if (Input.GetMouseButton(MouseButton.Right))
                    {
                        component.InvokeUIEvent(UIEvent.RightClick);
                        component.InvokeUIEvent(UIEvent.Click);
                    }
                    // ----------------------
                }
                else if (lastFrame.IsHovered == true)
                {
                    component.InvokeUIEvent(UIEvent.HoverEnd);
                }

                // Mouse button up
                if (Input.GetMouseButtonUp(MouseButton.Left) && thisFrame.IsLeftClicked)
                {
                    // This is set to false here because it should be true until the mouse button is released
                    thisFrame.IsLeftClicked = false;
                    component.InvokeUIEvent(UIEvent.LeftClickUp);
                    component.InvokeUIEvent(UIEvent.ClickUp);
                }

                if (Input.GetMouseButtonUp(MouseButton.Middle) && thisFrame.IsMiddleClicked)
                {
                    // This is set to false here because it should be true until the mouse button is released
                    thisFrame.IsMiddleClicked = false;
                    component.InvokeUIEvent(UIEvent.MiddleClickUp);
                    component.InvokeUIEvent(UIEvent.ClickUp);
                }

                if (Input.GetMouseButtonUp(MouseButton.Right) && thisFrame.IsRightClicked)
                {
                    // This is set to false here because it should be true until the mouse button is released
                    thisFrame.IsRightClicked = false;
                    component.InvokeUIEvent(UIEvent.RightClickUp);
                    component.InvokeUIEvent(UIEvent.ClickUp);
                }
                // ----------------------

                // Resetting the UiFrameTickData objects instead of destroying them
                thisFrame.IsClicked = thisFrame.IsLeftClicked || thisFrame.IsMiddleClicked || thisFrame.IsRightClicked;
                lastFrame.IsHovered = thisFrame.IsHovered;
                lastFrame.IsLeftClicked = thisFrame.IsLeftClicked;
                lastFrame.IsMiddleClicked = thisFrame.IsMiddleClicked;
                lastFrame.IsRightClicked = thisFrame.IsRightClicked;
                lastFrame.IsClicked = thisFrame.IsClicked;
                lastFrame.IsInteractable = thisFrame.IsInteractable;
            }
            if(hoveredCount == 0)
            {
                if(Input.GetMouseButtonDown(MouseButton.Left)
                    || Input.GetMouseButtonDown(MouseButton.Middle)
                    || Input.GetMouseButtonDown(MouseButton.Right))
                {
                    _focusedComponent?.InvokeUIEvent(UIEvent.LoseFocus);
                    _focusedComponent = null;
                }
            }
        }

        public void Focus(UIComponent component)
        {
            if (_focusedComponent == component) return;
            _focusedComponent?.InvokeUIEvent(UIEvent.LoseFocus);
            _focusedComponent = component;
            component.InvokeUIEvent(UIEvent.Focus);
            component.Focus(false);
        }

        public void Unfocus(UIComponent component)
        {
            if (component != _focusedComponent) return;
            component.InvokeUIEvent(UIEvent.LoseFocus);
            component.Unfocus(false);
            _focusedComponent = null;
        }
    }
}