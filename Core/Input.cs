using System.Numerics;
using Electron2D.Core.Rendering;
using GLFW;

namespace Electron2D.Core
{
    public static class Input
    {
        private static bool[] KEYS;
        private static bool[] KEYS_LAST;

        private static int totalKeyCount;
        private static Keys[] keyValues;

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
        }

        public static void ProcessInput()
        {
            // Resetting the KEYS_LAST table
            for (int i = 0; i < KEYS_LAST.Length; i++)
            {
                KEYS_LAST[i] = KEYS[i];
            }

            Glfw.PollEvents();

            // Looping through every key to see if it is being pressed or released
            for (int i = 0; i < totalKeyCount; i++)
            {
                if ((int)keyValues[i] == -1) continue; // Passing Unknown (-1) into the GetKey function causes an error
                KEYS[i] = Glfw.GetKey(DisplayManager.Instance.window, keyValues[i]) == InputState.Press;
            }
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


        public static Vector2 GetMousePosition()
        {
            double x;
            double y;
            Glfw.GetCursorPosition(DisplayManager.Instance.window, out x, out y);

            return new Vector2((float)x, (float)y);
        }
    }
}
