using System.Numerics;

namespace Electron2D.Rendering
{
    public class Camera2D
    {
        public static Camera2D Main { get; private set; }
        public float Zoom { get; set; }
        public Transform Transform { get; set; }

        public Camera2D(Vector2 startPosition, float zoom)
        {
            Transform = new Transform();
            Transform.Position = startPosition;

            Zoom = zoom;

            if (Main == null) Main = this;
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            float positionScale = Display.WindowScale;
            float left = (Transform.Position.X * positionScale) - Display.WindowSize.X / 2f;
            float right = (Transform.Position.X * positionScale) + Display.WindowSize.X / 2f;
            float top = (Transform.Position.Y * positionScale) + Display.WindowSize.Y / 2f;
            float bottom = (Transform.Position.Y * positionScale) - Display.WindowSize.Y / 2f;

            Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0.01f, 100f);
            Matrix4x4 zoomMatrix = Matrix4x4.CreateScale(Zoom);

            return orthoMatrix * zoomMatrix;
        }
        
        /// <summary>
        /// Returns an unscaled projection matrix that has no position and has default zoom.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 GetUnscaledProjectionMatrix()
        {
            float left = 0 - Display.WindowSize.X / 2f;
            float right = 0 + Display.WindowSize.X / 2f;
            float top = 0 + Display.WindowSize.Y / 2f;
            float bottom = 0 - Display.WindowSize.Y / 2f;

            Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0.01f, 100f);
            Matrix4x4 zoomMatrix = Matrix4x4.CreateScale(1);

            return orthoMatrix * zoomMatrix;
        }
    }
}
