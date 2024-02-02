using Electron2D.Core.ECS;
using System;
using System.Numerics;

namespace Electron2D.Core.Rendering
{
    public class Camera2D : Entity
    {
        public static Camera2D Main { get; private set; }
        public float Zoom { get; set; }
        public Transform Transform { get; set; }

        public Camera2D(Vector2 _startPosition, float _zoom)
        {
            Transform = new Transform();
            Transform.Position = _startPosition;
            AddComponent(Transform);

            Zoom = _zoom;

            if (Main == null) Main = this;
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            float positionScale = Game.WINDOW_SCALE;
            float left = (Transform.Position.X * positionScale) - DisplayManager.Instance.WindowSize.X / 2f;
            float right = (Transform.Position.X * positionScale) + DisplayManager.Instance.WindowSize.X / 2f;
            float top = (Transform.Position.Y * positionScale) + DisplayManager.Instance.WindowSize.Y / 2f;
            float bottom = (Transform.Position.Y * positionScale) - DisplayManager.Instance.WindowSize.Y / 2f;

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
            float left = 0 - DisplayManager.Instance.WindowSize.X / 2f;
            float right = 0 + DisplayManager.Instance.WindowSize.X / 2f;
            float top = 0 + DisplayManager.Instance.WindowSize.Y / 2f;
            float bottom = 0 - DisplayManager.Instance.WindowSize.Y / 2f;

            Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0.01f, 100f);
            Matrix4x4 zoomMatrix = Matrix4x4.CreateScale(1);

            return orthoMatrix * zoomMatrix;
        }
    }
}
