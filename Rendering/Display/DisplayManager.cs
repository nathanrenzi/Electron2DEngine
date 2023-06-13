using GLFW;
using System.Drawing;
using System.Numerics;
using static OpenGLTest.OpenGL.GL;

namespace OpenGLTest.Rendering.Display
{
    static class DisplayManager
    {
        public static Window window;
        public static Vector2 windowSize;

        public static void CreateWindow(int _width, int _height, string _title)
        {
            windowSize = new Vector2(_width, _height);

            Glfw.Init();

            // OpenGL 3.3 Core Profile
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);

            Glfw.WindowHint(Hint.Focused, true);
            Glfw.WindowHint(Hint.Resizable, false);

            window = Glfw.CreateWindow(_width, _height, _title, GLFW.Monitor.None, Window.None);

            if(window == Window.None)
            {
                // Error creating window
                return;
            }

            // Centering the window
            Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
            int x = (screen.Width - _width) / 2;
            int y = (screen.Height - _height) / 2;
            Glfw.SetWindowPosition(window, x, y);

            Glfw.MakeContextCurrent(window);
            Import(Glfw.GetProcAddress);

            glViewport(0, 0, _width, _height);
            Glfw.SwapInterval(1); // VSYNC is off, 1 is on
        }

        public static void CloseWindow()
        {
            Glfw.Terminate();
        }
    }
}
