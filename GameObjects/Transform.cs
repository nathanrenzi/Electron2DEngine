using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Electron2D.GameObjects
{
    public class Transform
    {
        private const float REFERENCE_WIDTH = 1920f;

        public Vector2 position;
        public Vector2 scale = Vector2.One;
        public double rotation;
        public float[] vertices =
        {
                // Position    UV        Color
                -1f, 1f,       0f, 0f,   1f, 0f, 0f,    // top left
                1f, 1f,        1f, 0f,   0f, 1f, 0f,    // top right
                -1f, -1f,      0f, 1f,   0f, 0f, 1f,    // bottom left

                1f, 1f,        1f, 0f,   0f, 1f, 0f,    // top right
                1f, -1f,       1f, 1f,   0f, 1f, 1f,    // bottom right
                -1f, -1f,      0f, 1f,   0f, 0f, 1f,    // bottom left
        };

        public Matrix4x4 GetPositionMatrix()
        {
            // Calculations below ensure that the position will scale with the screen width
            return Matrix4x4.CreateTranslation(position.X * (Engine.mainDriver.currentWindowWidth / REFERENCE_WIDTH) + (Engine.mainDriver.currentWindowWidth / 2f),
                position.Y * (Engine.mainDriver.currentWindowWidth / REFERENCE_WIDTH) + (Engine.mainDriver.currentWindowHeight / 2f), 0);
        }

        public Matrix4x4 GetScaleMatrix()
        {
            // Calculations below ensure that the scale will remain constant between screen sizes
            return Matrix4x4.CreateScale(Engine.mainDriver.currentWindowWidth * (scale.X / 100f), Engine.mainDriver.currentWindowWidth * (scale.Y / 100f), 1);
        }

        public Matrix4x4 GetRotationMatrix()
        {
            return Matrix4x4.CreateRotationZ((float)rotation);
        }
    }
}
