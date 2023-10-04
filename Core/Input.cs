using System.Numerics;
using Electron2D.Core.Rendering;
using GLFW;

namespace Electron2D.Core
{
    public static class Input
    {
        public static float scrollDelta;

        private static bool[] KEYS;
        private static bool[] KEYS_LAST;

        private static InputState[] MOUSE = new InputState[8];
        private static InputState[] MOUSE_LAST = new InputState[8];

        private static int totalKeyCount;
        private static Keys[] keyValues;

        private static MouseCallback scrollCallback; // MUST BE REFERENCED SO THAT GC DOESNT DESTROY CALLBACK
        private static bool scrollCallbackFrame = false;

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
            Glfw.SetScrollCallback(DisplayManager.Instance.Window, scrollCallback);
        }

        public static void ScrollCallback(Window _window, double _xOffset, double _yOffset)
        {
            scrollDelta = (float)_yOffset / 10;
            scrollCallbackFrame = true;
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
                scrollDelta = 0;
            }

            // Looping through every key to see if it is being pressed or released
            for (int i = 0; i < totalKeyCount; i++)
            {
                if ((int)keyValues[i] == -1) continue; // Passing Unknown (-1) into the GetKey function causes an error
                KEYS[i] = Glfw.GetKey(DisplayManager.Instance.Window, keyValues[i]) == InputState.Press;
            }
            for (int i = 0; i < MOUSE.Length; i++)
            {
                // Mouse presses
                MOUSE[i] = Glfw.GetMouseButton(DisplayManager.Instance.Window, (MouseButton)i);
            }

            scrollCallbackFrame = false;
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

        public static Vector2 GetMouseScreenPositionRaw(bool offsetToMiddle = false)
        {
            double x;
            double y;
            Glfw.GetCursorPosition(DisplayManager.Instance.Window, out x, out y);

            Vector2 offset = offsetToMiddle ? new Vector2(Program.game.CurrentWindowWidth / 2f, Program.game.CurrentWindowHeight / 2f) : Vector2.Zero;
            return new Vector2((float)x - offset.X, DisplayManager.Instance.WindowSize.Y - (float)y - offset.Y);
        }

        public static Vector2 GetMouseScreenPositionScaled(bool offsetToMiddle = false) => GetMouseScreenPositionRaw(offsetToMiddle) / Game.WINDOW_SCALE;

        public static Vector2 GetMouseWorldPosition()
        {
            Vector2 worldPosition = GetMouseScreenPositionRaw();

            // Centering the position
            worldPosition.Y -= Program.game.CurrentWindowHeight / 2;
            worldPosition.X -= Program.game.CurrentWindowWidth / 2;
            // ----------------------

            // Scaling the cursor
            worldPosition /= Game.WINDOW_SCALE;
            // ----------------------

            // Offsetting and scaling the position based on the current camera
            worldPosition /= Camera2D.main.zoom;
            worldPosition += (Camera2D.main.position / Game.WINDOW_SCALE);
            // ----------------------

            return new Vector2(worldPosition.X, worldPosition.Y);
        }
    }
}
