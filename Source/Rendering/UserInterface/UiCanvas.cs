﻿using System.Numerics;

namespace Electron2D.UserInterface
{
    public class UiCanvas
    {
        public class UiFrameTickData
        {
            public bool isHovered;
            public bool isLeftClicked;
            public bool isMiddleClicked;
            public bool isRightClicked;
            public bool isClicked;
            public bool isInteractable;
        }

        private List<UiComponent> activeComponents = new List<UiComponent>();

        // Add an ActiveUiComponent class that also holds a reference to the constraints being used
        //  and use that for the above list instead

        private bool initialized = false;

        public void Initialize()
        {
            if (initialized == true) return;
            initialized = true;

            Game.LateUpdateEvent += CheckForInput;
        }

        public void RegisterUiComponent(UiComponent _component)
        {
            if (activeComponents.Contains(_component)) return;

            _component.SetParentCanvas(this);
            activeComponents.Add(_component);
        }

        public void UnregisterUiComponent(UiComponent _component)
        {
            if (!activeComponents.Contains(_component)) return;

            activeComponents.Remove(_component);
        }

        public void CheckForInput()
        {
            Vector2 mousePos = Input.GetMouseWorldPosition();
            Vector2 mousePosScreen = Input.GetOffsetMousePositionScaled(true);
            for (int i = 0; i < activeComponents.Count; i++)
            {
                UiComponent component = activeComponents[i];
                UiFrameTickData lastFrame = activeComponents[i].LastFrameData;
                UiFrameTickData thisFrame = activeComponents[i].ThisFrameData;

                // Checking if UI Component has just disabled interactability
                thisFrame.isInteractable = activeComponents[i].Interactable;
                if (lastFrame.isInteractable == true && thisFrame.isInteractable == false)
                {
                    if (lastFrame.isHovered)
                    {
                        thisFrame.isHovered = false;
                        component.InvokeUiAction(UiEvent.HoverEnd);
                    }

                    component.InvokeUiAction(UiEvent.InteractabilityEnd);
                    Debug.Log("Interactability Ended...");
                }
                if (!activeComponents[i].Interactable)
                {
                    thisFrame.isClicked = false;
                    lastFrame.isHovered = false;
                    lastFrame.isLeftClicked = false;
                    lastFrame.isMiddleClicked = false;
                    lastFrame.isRightClicked = false;
                    lastFrame.isClicked = false;
                    lastFrame.isInteractable = false;
                    continue;
                }

                thisFrame.isHovered = activeComponents[i].CheckBounds(activeComponents[i].UseScreenPosition ? mousePosScreen : mousePos);
                if (thisFrame.isHovered)
                {
                    // Mouse is hovering over UI component
                    component.InvokeUiAction(UiEvent.Hover);

                    if (thisFrame.isHovered == true && lastFrame.isHovered == false)
                    {
                        component.InvokeUiAction(UiEvent.HoverStart);
                    }

                    // Left click
                    if (Input.GetMouseButtonDown(GLFW.MouseButton.Left))
                    {
                        thisFrame.isLeftClicked = true;
                        component.InvokeUiAction(UiEvent.LeftClickDown);
                        component.InvokeUiAction(UiEvent.ClickDown);
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
                        thisFrame.isMiddleClicked = true;
                        component.InvokeUiAction(UiEvent.MiddleClickDown);
                        component.InvokeUiAction(UiEvent.ClickDown);
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
                        thisFrame.isRightClicked = true;
                        component.InvokeUiAction(UiEvent.RightClickDown);
                        component.InvokeUiAction(UiEvent.ClickDown);
                    }

                    if (Input.GetMouseButton(GLFW.MouseButton.Right))
                    {
                        component.InvokeUiAction(UiEvent.RightClick);
                        component.InvokeUiAction(UiEvent.Click);
                    }
                    // ----------------------
                }
                else if (lastFrame.isHovered == true)
                {
                    component.InvokeUiAction(UiEvent.HoverEnd);
                }

                // Mouse button up
                if (Input.GetMouseButtonUp(GLFW.MouseButton.Left) && thisFrame.isLeftClicked)
                {
                    // This is set to false here because it should be true until the mouse button is released
                    thisFrame.isLeftClicked = false;
                    component.InvokeUiAction(UiEvent.LeftClickUp);
                    component.InvokeUiAction(UiEvent.ClickUp);
                }

                if (Input.GetMouseButtonUp(GLFW.MouseButton.Middle) && thisFrame.isMiddleClicked)
                {
                    // This is set to false here because it should be true until the mouse button is released
                    thisFrame.isMiddleClicked = false;
                    component.InvokeUiAction(UiEvent.MiddleClickUp);
                    component.InvokeUiAction(UiEvent.ClickUp);
                }

                if (Input.GetMouseButtonUp(GLFW.MouseButton.Right) && thisFrame.isRightClicked)
                {
                    // This is set to false here because it should be true until the mouse button is released
                    thisFrame.isRightClicked = false;
                    component.InvokeUiAction(UiEvent.RightClickUp);
                    component.InvokeUiAction(UiEvent.ClickUp);
                }
                // ----------------------

                // Resetting the UiFrameTickData objects instead of destroying them
                thisFrame.isClicked = thisFrame.isLeftClicked || thisFrame.isMiddleClicked || thisFrame.isRightClicked;
                lastFrame.isHovered = thisFrame.isHovered;
                lastFrame.isLeftClicked = thisFrame.isLeftClicked;
                lastFrame.isMiddleClicked = thisFrame.isMiddleClicked;
                lastFrame.isRightClicked = thisFrame.isRightClicked;
                lastFrame.isClicked = thisFrame.isClicked;
                lastFrame.isInteractable = thisFrame.isInteractable;
            }
        }
    }
}