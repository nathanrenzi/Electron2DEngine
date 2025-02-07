using Electron2D.Misc.Input;
using Electron2D.Rendering;
using GLFW;
using System.Numerics;

namespace Electron2D
{
    public static class Input
    {
        public static float ScrollDelta;
        public static Vector2 MousePosition;

        private static bool[] KEYS;
        private static bool[] KEYS_LAST;
        private static InputState[] MOUSE = new InputState[8];
        private static InputState[] MOUSE_LAST = new InputState[8];

        private static int _totalKeyCount;
        private static KeyCode[] _keyValues;

        private static MouseCallback _scrollCallback;
        private static bool _scrollCallbackFrame = false;

        private static MouseCallback _mouseCallback;
        private static CharCallback _charCallback;

        private static List<IKeyListener> _keyListeners = new List<IKeyListener>();
        private static bool _lockKeyInput = false;

        private static Dictionary<uint, Dictionary<object, bool>> _lockPriorityDictionary = new();

        public static void AddListener(IKeyListener listener)
        {
            if(_keyListeners.Contains(listener)) return;
            _keyListeners.Add(listener);
        }

        public static void RemoveListener(IKeyListener listener)
        {
            _keyListeners.Remove(listener);
        }

        /// <summary>
        /// Locks the key input.
        /// Only <see cref="IKeyListener"/> instances will receive key inputs.
        /// </summary>
        public static bool LockKeyInput(object callerObj, uint priority = 0)
        {
            if(!_lockPriorityDictionary.ContainsKey(priority))
            {
                _lockPriorityDictionary.Add(priority, new Dictionary<object, bool>());
            }
            if (!_lockPriorityDictionary[priority].ContainsKey(callerObj))
            {
                _lockPriorityDictionary[priority].Add(callerObj, true);
            }
            else
            {
                _lockPriorityDictionary[priority][callerObj] = true;
            }
            return RecalculateLock();
        }

        /// <summary>
        /// Unlocks the key input.
        /// </summary>
        public static bool UnlockKeyInput(object callerObj, uint priority = 0)
        {
            if (!_lockPriorityDictionary.ContainsKey(priority))
            {
                _lockPriorityDictionary.Add(priority, new Dictionary<object, bool>());
            }
            if (!_lockPriorityDictionary[priority].ContainsKey(callerObj))
            {
                _lockPriorityDictionary[priority].Add(callerObj, false);
            }
            else
            {
                _lockPriorityDictionary[priority][callerObj] = false;
            }
            return RecalculateLock();
        }

        private static bool RecalculateLock()
        {
            uint lastMaxPriority = 0;
            uint maxPriority = 0;
            bool shouldLock = false;
            foreach (var pair in _lockPriorityDictionary)
            {
                if(pair.Key >= maxPriority)
                {
                    bool foundEntryInThisPriority = false;
                    lastMaxPriority = maxPriority;
                    maxPriority = pair.Key;
                    foreach (var value in pair.Value)
                    {
                        if (value.Key == null) continue;
                        foundEntryInThisPriority = true;
                        shouldLock = value.Value;
                    }
                    if(!foundEntryInThisPriority)
                    {
                        maxPriority = lastMaxPriority;
                        pair.Value.Clear();
                    }
                }
            }
            _lockKeyInput = shouldLock;
            return shouldLock;
        }

        public static void Initialize()
        {
            _totalKeyCount = Enum.GetNames<KeyCode>().Length;
            _keyValues = Enum.GetValues<KeyCode>();

            KEYS = new bool[_totalKeyCount];
            KEYS_LAST = new bool[_totalKeyCount];

            for (int i = 0; i < KEYS.Length; i++)
            {
                KEYS[i] = false;
            }

            _scrollCallback = new MouseCallback(ScrollCallback);
            Glfw.SetScrollCallback(Display.Window, _scrollCallback);

            _mouseCallback = new MouseCallback(MouseCallback);
            Glfw.SetCursorPositionCallback(Display.Window, _mouseCallback);

            _charCallback = new CharCallback(CharCallback);
            Glfw.SetCharCallback(Display.Window, _charCallback);
        }

        private static void CharCallback(Window window, uint charCode)
        {
            for(int i = 0; i < _keyListeners.Count; i++)
            {
                _keyListeners[i].KeyPressed((char)charCode);
            }
        }

        private static void ScrollCallback(Window window, double xOffset, double yOffset)
        {
            ScrollDelta = (float)yOffset / 10;
            _scrollCallbackFrame = true;
        }

        private static void MouseCallback(Window window, double x, double y)
        {
            MousePosition = new Vector2((float)x, (float)y);
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
            if (!_scrollCallbackFrame)
            {
                // If the scroll delta was not updated this frame, set the delta to 0
                ScrollDelta = 0;
            }

            // Looping through every key to see if it is being pressed or released
            for (int i = 0; i < _totalKeyCount; i++)
            {
                if ((int)_keyValues[i] == -1) continue; // Passing Unknown (-1) into the GetKey function causes an error
                KEYS[i] = Glfw.GetKey(Display.Window, (Keys)_keyValues[i]) == InputState.Press;
                if (!char.IsAscii((char)_keyValues[i]))
                {
                    if (KEYS[i] && !KEYS_LAST[i])
                    {
                        for (int x = 0; x < _keyListeners.Count; x++)
                        {
                            _keyListeners[x].KeyPressed((char)_keyValues[i]);
                        }
                    }
                    else if(!KEYS[i] && KEYS_LAST[i])
                    {
                        for (int x = 0; x < _keyListeners.Count; x++)
                        {
                            _keyListeners[x].KeyNonASCIIReleased((char)_keyValues[i]);
                        }
                    }
                }
            }
            for (int i = 0; i < MOUSE.Length; i++)
            {
                // Mouse presses
                MOUSE[i] = Glfw.GetMouseButton(Display.Window, (GLFW.MouseButton)i);
            }

            _scrollCallbackFrame = false;
        }

        public static bool GetMouseButtonDown(MouseButton button)
        {
            return MOUSE[(int)button] == InputState.Press && MOUSE_LAST[(int)button] == InputState.Release;
        }

        public static bool GetMouseButtonUp(MouseButton button)
        {
            return MOUSE[(int)button] == InputState.Release && MOUSE_LAST[(int)button] == InputState.Press;
        }

        public static bool GetMouseButton(MouseButton button)
        {
            return MOUSE[(int)button] == InputState.Press;
        }

        public static bool GetKey(KeyCode key)
        {
            int index = GetKeyIndex(key);
            if (index == -1) return false;

            return _lockKeyInput ? false : KEYS[index];
        }

        public static bool GetKeyDown(KeyCode key)
        {
            int index = GetKeyIndex(key);
            if (index == -1) return false;

            return _lockKeyInput ? false : KEYS[index] && !KEYS_LAST[index];
        }

        public static bool GetKeyUp(KeyCode key)
        {
            int index = GetKeyIndex(key);
            if (index == -1) return false;

            return _lockKeyInput ? false : !KEYS[index] && KEYS_LAST[index];
        }

        private static int GetKeyIndex(KeyCode key)
        {
            for (int i = 0; i < _keyValues.Length; i++)
            {
                if (_keyValues[i] == key)
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
