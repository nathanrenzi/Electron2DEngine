using Electron2D.Rendering;
using GLFW;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D
{
    public static class Display
    {
        public const float REFERENCE_WINDOW_WIDTH = 1920f;
        public const float REFERENCE_WINDOW_HEIGHT = 1080f;
        public static event Action OnWindowResize;

        public static Window Window { get; private set; }
        public static Vector2 WindowSize { get; private set; }
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
            Glfw.WindowHint(Hint.RefreshRate, Engine.Game.Settings.RefreshRate);
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

            if (File.Exists(ResourceManager.GetEngineResourcePath("icon.ico")))
            {
                Texture2D texture = ResourceManager.Instance.LoadTexture(ResourceManager.GetEngineResourcePath("icon.ico"));
                SetIcon(texture);
            }
            Settings settings = Engine.Game.Settings;
            SetWindowMode(settings.WindowMode);
        }

        /// <summary>
        /// Can be used to set the icon of the window during runtime. Note: To change the icon of the .exe file and window at startup,
        /// be sure to place a default icon file at 'Resources/icon.ico'
        /// </summary>
        /// <param name="iconTexture">The texture of the icon to use. Note: Must be 16x16, 32x32, 48x48, 64x64, 128x128, or 256x256. GLFW will automatically
        /// resize if needed, so use a larger resolution.</param>
        public static void SetIcon(Texture2D iconTexture)
        {
            Bitmap bitmap = iconTexture.GetData(GL_RGBA);
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
            var data = bitmap.LockBits(
            new Rectangle(0, 0, iconTexture.Width, iconTexture.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);
            GLFW.Image image = new GLFW.Image(iconTexture.Width, iconTexture.Height, data.Scan0);
            Glfw.SetWindowIcon(Window, 1, new GLFW.Image[] { image });
        }

        public static void SetWindowMode(WindowMode mode)
        {
            Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
            Settings settings = Engine.Game.Settings;
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
            OnWindowResize?.Invoke();
        }

        public static void DestroyWindow()
        {
            Glfw.DestroyWindow(Window);
            Window = Window.None;
        }
    }
}
