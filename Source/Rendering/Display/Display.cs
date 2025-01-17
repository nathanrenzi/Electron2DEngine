using Electron2D.Rendering;
using GLFW;
using System.Drawing;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D
{
    public static class Display
    {
        public const float REFERENCE_WINDOW_WIDTH = 1920f;

        public static Window Window { get; private set; }
        public static Vector2 WindowSize { get; private set; }
        public static float WindowScale
        {
            get
            {
                return WindowSize.X / REFERENCE_WINDOW_WIDTH;
            }
        }
        private static WindowMode _windowMode = WindowMode.None;

        public static void Initialize()
        {
            Glfw.Init();
        }

        public static void CreateWindow(int width, int height, string title)
        {
            if(Window != Window.None)
            {
                Debug.LogError("Cannot create a window. Multiple windows are not supported.");
                return;
            }

            if (width <= 0 || height <= 0)
            {
                width = Glfw.PrimaryMonitor.WorkArea.Width;
                height = Glfw.PrimaryMonitor.WorkArea.Height;
            }

            VideoMode mode = Glfw.GetVideoMode(Glfw.PrimaryMonitor);

            // OpenGL 3.3 Core Profile
            Glfw.WindowHint(Hint.RedBits, mode.RedBits);
            Glfw.WindowHint(Hint.GreenBits, mode.GreenBits);
            Glfw.WindowHint(Hint.BlueBits, mode.BlueBits);
            Glfw.WindowHint(Hint.RefreshRate, Program.Game.Settings.RefreshRate);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            Glfw.WindowHint(Hint.Focused, true);
            Glfw.WindowHint(Hint.Resizable, false);

            Window = Glfw.CreateWindow(width, height, title, GLFW.Monitor.None, Window.None);

            if (Window == Window.None)
            {
                // Error creating window
                Console.WriteLine("Error creating window.");
                return;
            }

            Glfw.MakeContextCurrent(Window);
            Import(Glfw.GetProcAddress);

            Settings settings = Program.Game.Settings;
            SetWindowMode(settings.WindowMode);
        }

        public static void SetWindowMode(WindowMode mode)
        {
            Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
            Settings settings = Program.Game.Settings;
            switch (mode)
            {
                default:
                case WindowMode.Fullscreen:
                    Glfw.SetWindowMonitor(Window, Glfw.PrimaryMonitor, 0, 0, screen.Width,
                        screen.Height, settings.RefreshRate);
                    glViewport(0, 0, screen.Width, screen.Height);
                    WindowSize = new Vector2(screen.Width, screen.Height);
                    break;
                case WindowMode.Windowed:
                    Glfw.SetWindowAttribute(Window, WindowAttribute.Decorated, true);
                    Glfw.SetWindowMonitor(Window, GLFW.Monitor.None,
                        (screen.Width - settings.WindowWidth) / 2,
                        (screen.Height - settings.WindowHeight) / 2,
                        settings.WindowWidth,
                        settings.WindowHeight, settings.RefreshRate);
                    glViewport(0, 0, settings.WindowWidth, settings.WindowHeight);
                    WindowSize = new Vector2(settings.WindowWidth, settings.WindowHeight);
                    break;
                case WindowMode.BorderlessWindow:
                    Glfw.SetWindowAttribute(Window, WindowAttribute.Decorated, false);
                    Glfw.SetWindowMonitor(Window, GLFW.Monitor.None, 0, 0, screen.Width,
                        screen.Height, settings.RefreshRate);
                    glViewport(0, 0, screen.Width, screen.Height);
                    WindowSize = new Vector2(screen.Width, screen.Height);
                    break;
            }
        }

        public static void DestroyWindow()
        {
            Glfw.DestroyWindow(Window);
            Window = Window.None;
        }
    }
}
