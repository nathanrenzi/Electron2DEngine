using System.ComponentModel;
using System.Numerics;

namespace Electron2D.Core.UI
{
    public class UiDisplay
    {
        private List<UiComponent> activeComponents = new List<UiComponent>();
        private List<bool> wasHoveringThisFrame = new List<bool>();
        private List<bool> wasHoveringLastFrame = new List<bool>();
        // Add an ActiveUiComponent class that also holds a reference to the constraints being used
        //  and use that for the above list instead

        private bool initialized = false;

        public void Initialize()
        {
            if (initialized == true) return;
            initialized = true;

            Game.onLateUpdateEvent += CheckForInput;
        }

        public void RegisterUiComponent(UiComponent _component/*, UiConstraint _constraint*/)
        {
            if (activeComponents.Contains(_component)) return;

            activeComponents.Add(_component);
            wasHoveringThisFrame.Add(false);
            wasHoveringLastFrame.Add(false);
        }

        public void UnregisterUiComponent(UiComponent _component)
        {
            if (!activeComponents.Contains(_component)) return;
            int index = activeComponents.IndexOf(_component);

            wasHoveringThisFrame.RemoveAt(index);
            wasHoveringLastFrame.RemoveAt(index);
            activeComponents.Remove(_component);
        }

        public void CheckForInput()
        {
            Vector2 mousePos = Input.GetMouseWorldPosition();
            for (int i = 0; i < activeComponents.Count; i++)
            {
                UiComponent component = activeComponents[i];

                wasHoveringThisFrame[i] = activeComponents[i].CheckBounds(mousePos);
                if (wasHoveringThisFrame[i])
                {
                    // Mouse is hovering over UI component
                    component.InvokeUiAction(UiEvent.Hover);

                    if (wasHoveringThisFrame[i] == true && wasHoveringLastFrame[i] == false)
                    {
                        component.InvokeUiAction(UiEvent.HoverStart);
                    }

                    // Left click
                    if (Input.GetMouseButtonDown(GLFW.MouseButton.Left))
                    {
                        component.InvokeUiAction(UiEvent.LeftClickDown);
                        component.InvokeUiAction(UiEvent.ClickDown);
                    }
                    else if (Input.GetMouseButtonUp(GLFW.MouseButton.Left))
                    {
                        component.InvokeUiAction(UiEvent.LeftClickUp);
                        component.InvokeUiAction(UiEvent.ClickUp);
                    }

                    if (Input.GetMouseButton(GLFW.MouseButton.Left))
                    {
                        component.InvokeUiAction(UiEvent.LeftClick);
                        component.InvokeUiAction(UiEvent.Click);
                    }
                    // ----------------------

                    // Middle click
                    if (Input.GetMouseButtonDown(GLFW.MouseButton.Middle))
                    {
                        component.InvokeUiAction(UiEvent.MiddleClickDown);
                        component.InvokeUiAction(UiEvent.ClickDown);
                    }
                    else if (Input.GetMouseButtonUp(GLFW.MouseButton.Middle))
                    {
                        component.InvokeUiAction(UiEvent.MiddleClickUp);
                        component.InvokeUiAction(UiEvent.ClickUp);
                    }

                    if (Input.GetMouseButton(GLFW.MouseButton.Middle))
                    {
                        component.InvokeUiAction(UiEvent.MiddleClick);
                        component.InvokeUiAction(UiEvent.Click);
                    }
                    // ----------------------

                    // Right click
                    if (Input.GetMouseButtonDown(GLFW.MouseButton.Right))
                    {
                        component.InvokeUiAction(UiEvent.RightClickDown);
                        component.InvokeUiAction(UiEvent.ClickDown);
                    }
                    else if (Input.GetMouseButtonUp(GLFW.MouseButton.Right))
                    {
                        component.InvokeUiAction(UiEvent.RightClickUp);
                        component.InvokeUiAction(UiEvent.ClickUp);
                    }

                    if (Input.GetMouseButton(GLFW.MouseButton.Right))
                    {
                        component.InvokeUiAction(UiEvent.RightClick);
                        component.InvokeUiAction(UiEvent.Click);
                    }
                    // ----------------------
                }
                else if (wasHoveringLastFrame[i] == true)
                {
                    component.InvokeUiAction(UiEvent.HoverEnd);
                }

                wasHoveringLastFrame[i] = wasHoveringThisFrame[i];
            }
        }
    }
}