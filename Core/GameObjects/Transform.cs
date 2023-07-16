using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Electron2D.Core.GameObjects
{
    public class Transform
    {
        public const float REFERENCE_WINDOW_WIDTH = 1920f;

        public Vector2 position;
        public Vector2 scale = Vector2.One;
        public Vector2 pivotPoint = Vector2.Zero;
        public float rotation;

        public Vector2 up { get { return NormalizeVector(new Vector2(MathF.Cos(rotation * MathF.PI / 180), MathF.Sin(rotation * MathF.PI / 180))); } }
        public Vector2 right { get { return NormalizeVector(new Vector2(MathF.Cos((rotation - 90) * MathF.PI / 180), MathF.Sin((rotation - 90) * MathF.PI / 180))); } }

        public Matrix4x4 GetPositionMatrix()
        {
            // Calculations below ensure that the position will scale with the screen width
            return Matrix4x4.CreateTranslation(position.X * (Program.game.currentWindowWidth / REFERENCE_WINDOW_WIDTH), position.Y * (Program.game.currentWindowWidth / REFERENCE_WINDOW_WIDTH), 0);
        }

        public Matrix4x4 GetScaleMatrix()
        {
            // Calculations below ensure that the scale will remain constant between screen sizes
            return Matrix4x4.CreateScale(Program.game.currentWindowWidth * (scale.X / 100f), Program.game.currentWindowWidth * (scale.Y / 100f), 1);
        }

        public Matrix4x4 GetRotationMatrix()
        {
            // Converting from degrees to radians
            return Matrix4x4.CreateRotationZ(rotation * (MathF.PI / 180));
        }

        private Vector2 NormalizeVector(Vector2 _vector)
        {
            float length = MathF.Sqrt(_vector.X * _vector.X + _vector.Y * _vector.Y);
            return _vector / length;
        }
    }
}
