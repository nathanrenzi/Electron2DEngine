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

        /// <summary>
        /// Converts world coordinates to view space (camera-relative).
        /// </summary>
        public Matrix4x4 GetViewMatrix()
        {
            Matrix4x4 translation = Matrix4x4.CreateTranslation(-Transform.Position.X, -Transform.Position.Y, 0);
            Matrix4x4 rotation = Matrix4x4.CreateRotationZ(-Transform.Rotation * MathF.PI / 180f);
            Matrix4x4 zoom = Matrix4x4.CreateScale(Zoom, Zoom, 1f);

            return translation * rotation * zoom;
        }

        /// <summary>
        /// Creates an orthographic projection based on the current window size.
        /// </summary>
        public Matrix4x4 GetProjectionMatrix()
        {
            float halfWidth = Display.WindowSize.X / 2f;
            float halfHeight = Display.WindowSize.Y / 2f;

            return Matrix4x4.CreateOrthographicOffCenter(
                -halfWidth, halfWidth,
                -halfHeight, halfHeight,
                -1f, 1f
            );
        }

        /// <summary>
        /// Creates a combined view-projection matrix used for rendering.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 GetViewProjectionMatrix()
        {
            return GetViewMatrix() * GetProjectionMatrix();
        }
        
        /// <summary>
        /// Creates an unscaled orthographic projection for UI or screen-space rendering.
        /// </summary>
        public Matrix4x4 GetUnscaledProjectionMatrix()
        {
            return Matrix4x4.CreateOrthographicOffCenter(
                0f, Display.WindowSize.X,
                Display.WindowSize.Y, 0f,
                -1f, 1f
            );
        }
    }
}
