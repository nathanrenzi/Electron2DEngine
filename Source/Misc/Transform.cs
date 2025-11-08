using System.Numerics;

namespace Electron2D
{
    public class Transform
    {
        public event Action OnPositionChanged;

        public Vector2 Position
        {
            get => position;
            set
            {
                position = value;
                OnPositionChanged?.Invoke();
            }
        }
        private Vector2 position;

        public Vector2 Scale = new Vector2(100, 100);

        public float Rotation;

        public Vector2 Up => new(MathF.Sin(Radians), -MathF.Cos(Radians));
        public Vector2 Right => new(MathF.Cos(Radians), MathF.Sin(Radians));
        private float Radians => Rotation * MathF.PI / 180f;

        public Transform()
        {
            Scale = Vector2.One;
        }

        public Matrix4x4 GetPositionMatrix()
        {
            return Matrix4x4.CreateTranslation(Position.X, Position.Y, 0);
        }

        public Matrix4x4 GetScaleMatrix()
        {
            return Matrix4x4.CreateScale(Scale.X / 2f, Scale.Y / 2f, 1);
        }

        public Matrix4x4 GetRotationMatrix()
        {
            return Matrix4x4.CreateRotationZ(Radians);
        }
    }
}
