using GLFW;
using System.Drawing;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    public sealed class DisplayManager
    {
        private static DisplayManager instance = null;
        private static readonly object loc = new();
        public Window Window;
        public Vector2 WindowSize;

        public static DisplayManager Instance
        {
            get
            {
                lock(loc)
                {
                    if (instance is null)
                    { 
                        instance = new DisplayManager();
                    }
                    return instance;
                }
            }
        }

        public void CreateWindow(int _width, int _height, string _title, bool _antialiasing, bool _errorChecking)
        {
            WindowSize = new Vector2(_width, _height);

            Glfw.Init();

            // OpenGL 3.3 Core Profile
            if(_antialiasing) Glfw.WindowHint(Hint.Samples, 16); // Antialiasing
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            if(_errorChecking) Glfw.WindowHint(Hint.OpenglDebugContext, true);
            Glfw.WindowHint(Hint.Focused, true);
            Glfw.WindowHint(Hint.Resizable, false);

            Window = Glfw.CreateWindow(_width, _height, _title, GLFW.Monitor.None, Window.None);

            if (Window == Window.None)
            {
                // Error creating window
                Console.WriteLine("Error creating window.");
                return;
            }

            // Centering the window
            Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
            int x = (screen.Width - _width) / 2;
            int y = (screen.Height - _height) / 2;
            Glfw.SetWindowPosition(Window, x, y);
            Glfw.SetWindowMonitor(Window, Glfw.PrimaryMonitor, x, y, screen.Width, screen.Height, 240);

            Glfw.MakeContextCurrent(Window);
            Import(Glfw.GetProcAddress);

            glViewport(0, 0, _width, _height); // Call this again if window is resized
        }

        public static void CloseWindow()
        {
            Glfw.Terminate();
        }
    }
}
