using System.Numerics;
using Electron2D.Core.Rendering;
using GLFW;

namespace Electron2D.Core
{
    public static class Input
    {
        public static float ScrollDelta;
        public static Vector2 MousePosition;

        private static bool[] KEYS;
        private static bool[] KEYS_LAST;

        private static InputState[] MOUSE = new InputState[8];
        private static InputState[] MOUSE_LAST = new InputState[8];

        private static int totalKeyCount;
        private static Keys[] keyValues;

        private static MouseCallback scrollCallback; // MUST BE REFERENCED SO THAT GC DOESNT DESTROY CALLBACK
        private static bool scrollCallbackFrame = false;

        private static MouseCallback mouseCallback;
        private static bool mouseCallbackFrame = false;

        public static void Initialize()
        {
            totalKeyCount = Enum.GetNames<Keys>().Length;
            keyValues = Enum.GetValues<Keys>();

            KEYS = new bool[totalKeyCount];
            KEYS_LAST = new bool[totalKeyCount];

            for (int i = 0; i < KEYS.Length; i++)
            {
                KEYS[i] = false;
            }

            scrollCallback = new MouseCallback(ScrollCallback);
            Glfw.SetScrollCallback(Display.Window, scrollCallback);

            mouseCallback = new MouseCallback(MouseCallback);
            Glfw.SetCursorPositionCallback(Display.Window, mouseCallback);
        }

        public static void ScrollCallback(Window _window, double _xOffset, double _yOffset)
        {
            ScrollDelta = (float)_yOffset / 10;
            scrollCallbackFrame = true;
        }

        public static void MouseCallback(Window _window, double _x, double _y)
        {
            MousePosition = new Vector2((float)_x, (float)_y);
        }

        public static void ProcessInput()
        {
            // Resetting the LAST tables
            for (int i = 0; i < KEYS_LAST.Length; i++)
            {
                KEYS_LAST[i] = KEYS[i];
            }
            for (int i = 0; i < MOUSE_LAST.Length; i++)
            {
                MOUSE_LAST[i] = MOUSE[i];
            }

            Glfw.PollEvents();
            if (!scrollCallbackFrame)
            {
                // If the scroll delta was not updated this frame, set the delta to 0
                ScrollDelta = 0;
            }
            if(!mouseCallbackFrame)
            {
                MousePosition = Vector2.Zero;
            }

            // Looping through every key to see if it is being pressed or released
            for (int i = 0; i < totalKeyCount; i++)
            {
                if ((int)keyValues[i] == -1) continue; // Passing Unknown (-1) into the GetKey function causes an error
                KEYS[i] = Glfw.GetKey(Display.Window, keyValues[i]) == InputState.Press;
            }
            for (int i = 0; i < MOUSE.Length; i++)
            {
                // Mouse presses
                MOUSE[i] = Glfw.GetMouseButton(Display.Window, (MouseButton)i);
            }

            scrollCallbackFrame = false;
            mouseCallbackFrame = false;
        }

        public static bool GetMouseButtonDown(MouseButton _button)
        {
            return MOUSE[(int)_button] == InputState.Press && MOUSE_LAST[(int)_button] == InputState.Release;
        }

        public static bool GetMouseButtonUp(MouseButton _button)
        {
            return MOUSE[(int)_button] == InputState.Release && MOUSE_LAST[(int)_button] == InputState.Press;
        }

        public static bool GetMouseButton(MouseButton _button)
        {
            return MOUSE[(int)_button] == InputState.Press;
        }

        public static bool GetKey(Keys _key)
        {
            int index = GetKeyIndex(_key);
            if (index == -1) return false;

            return KEYS[index];
        }

        public static bool GetKeyDown(Keys _key)
        {
            int index = GetKeyIndex(_key);
            if (index == -1) return false;

            return KEYS[index] && !KEYS_LAST[index];
        }

        public static bool GetKeyUp(Keys _key)
        {
            int index = GetKeyIndex(_key);
            if (index == -1) return false;

            return !KEYS[index] && KEYS_LAST[index];
        }

        private static int GetKeyIndex(Keys _key)
        {
            for (int i = 0; i < keyValues.Length; i++)
            {
                if (keyValues[i] == _key)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the mouse position, offset to the middle of the window.
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetOffsetMousePosition()
        {
            Vector2 offset = Display.WindowSize / 2f;
            return new Vector2((float)MousePosition.X - offset.X,
                Display.WindowSize.Y - (float)MousePosition.Y - offset.Y);
        }

        /// <summary>
        /// Gets the mouse position, offset to the middle of the window and scaled to match the WindowScale
        /// </summary>
        /// <param name="offsetToMiddle"></param>
        /// <returns></returns>
        public static Vector2 GetOffsetMousePositionScaled(bool offsetToMiddle = false) => GetOffsetMousePosition() / Display.WindowScale;

        public static Vector2 GetMouseWorldPosition()
        {
            Vector2 worldPosition = GetOffsetMousePosition();

            // Centering the position
            worldPosition.Y -= Display.WindowSize.Y / 2;
            worldPosition.X -= Display.WindowSize.X / 2;
            // ----------------------

            // Scaling the cursor
            worldPosition /= Display.WindowScale;
            // ----------------------

            // Offsetting and scaling the position based on the current camera
            worldPosition /= Camera2D.Main.Zoom;
            worldPosition += (Camera2D.Main.Transform.Position / Display.WindowScale);
            // ----------------------

            return new Vector2(worldPosition.X, worldPosition.Y);
        }
    }
}
