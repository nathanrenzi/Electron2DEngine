﻿using System;
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
            float positionScale = Game.WINDOW_SCALE;
            float left = (position.X * positionScale) - DisplayManager.Instance.WindowSize.X / 2f;
            float right = (position.X * positionScale) + DisplayManager.Instance.WindowSize.X / 2f;
            float top = (position.Y * positionScale) + DisplayManager.Instance.WindowSize.Y / 2f;
            float bottom = (position.Y * positionScale) - DisplayManager.Instance.WindowSize.Y / 2f;

            Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0.01f, 100f);
            Matrix4x4 zoomMatrix = Matrix4x4.CreateScale(zoom);

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
