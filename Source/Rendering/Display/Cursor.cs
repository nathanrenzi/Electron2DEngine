using Electron2D.Rendering;
using GLFW;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace Electron2D
{
    public static class Cursor
    {
        public static CursorLockMode LockMode { get; private set; }
        public static bool Visible { get; private set; }

        private static Dictionary<CursorType, GLFW.Cursor> _defaultCursorDictionary = new();
        private static Dictionary<(Texture2D, Vector2), GLFW.Cursor> _texturedCursorDictionary = new();

        public static void Initialize()
        {
            Visible = true;
            GLFW.Cursor defaultCursor = Glfw.CreateStandardCursor(CursorType.Arrow);
            _defaultCursorDictionary.Add(CursorType.Arrow, defaultCursor);
        }

        public static void SetType(CursorType type)
        {
            if(_defaultCursorDictionary.ContainsKey(type))
            {
                Glfw.SetCursor(Display.Window, _defaultCursorDictionary[type]);
            }
            else
            {
                _defaultCursorDictionary.Add(type, Glfw.CreateStandardCursor(type));
                Glfw.SetCursor(Display.Window, _defaultCursorDictionary[type]);
            }
        }

        /// <summary>
        /// Sets the texture of the cursor.
        /// </summary>
        /// <param name="texture">The texture to be applied to the cursor.</param>
        /// <param name="hotspot">The hotspot of the cursor in pixels (where the point will be).
        /// X is left to right and Y is top to bottom.</param>
        public static void SetTexture(Texture2D texture, Vector2 hotspot)
        {
            if(_texturedCursorDictionary.ContainsKey((texture, hotspot)))
            {
                Glfw.SetCursor(Display.Window, _texturedCursorDictionary[(texture, hotspot)]);
            }
            else
            {
                Bitmap bitmap = texture.GetData(OpenGL.GL.GL_RGBA);
                bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
                var data = bitmap.LockBits(
                        new Rectangle(0, 0, texture.Width, texture.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);
                GLFW.Image image = new GLFW.Image(texture.Width, texture.Height, data.Scan0);
                GLFW.Cursor cursor = Glfw.CreateCursor(image, (int)hotspot.X, (int)hotspot.Y);
                bitmap.UnlockBits(data);
                _texturedCursorDictionary.Add((texture, hotspot), cursor);
                Glfw.SetCursor(Display.Window, cursor);
            }
        }

        public static void SetLockMode(CursorLockMode mode)
        {
            LockMode = mode;
            UpdateCursorState();
        }

        public static void SetVisible(bool visible)
        {
            Visible = visible;
            UpdateCursorState();
        }

        private static void UpdateCursorState()
        {
            Glfw.SetInputMode(Display.Window, InputMode.Cursor, (int)(LockMode == CursorLockMode.Locked ? CursorMode.Disabled :
                (Visible ? CursorMode.Normal : CursorMode.Hidden)));
            // If this were a 3d application, enable raw mouse movement if available for more accurate movement when locked
        }
    }
}
