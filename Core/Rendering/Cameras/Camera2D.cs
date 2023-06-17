using System;
using System.Numerics;

namespace Electron2D.Core.Rendering
{
    public class Camera2D
    {
        public static Camera2D main { get; private set; }

        public Vector2 position;
        public float zoom;

        public Camera2D(Vector2 _focusPosition, float _zoom)
        {
            position = _focusPosition;
            zoom = _zoom;

            if (main == null) main = this;
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            float left = position.X + DisplayManager.Instance.windowSize.X / 2f;
            float right = position.X - DisplayManager.Instance.windowSize.X / 2f;
            float top = position.Y + DisplayManager.Instance.windowSize.Y / 2f;
            float bottom = position.Y - DisplayManager.Instance.windowSize.Y / 2f;

            Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0.01f, 100f);
            Matrix4x4 zoomMatrix = Matrix4x4.CreateScale(zoom);

            return orthoMatrix * zoomMatrix;
        }
    }
}
