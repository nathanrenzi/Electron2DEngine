using Electron2D.Core.ECS;
using System.Numerics;

namespace Electron2D.Core
{
    public class TransformSystem : BaseSystem<Transform> { }
    public class Transform : Component
    {
        public Action OnPositionChanged { get; set; }

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

        public Transform()
        {
            Scale = Vector2.One;
            TransformSystem.Register(this);
        }

        ~Transform() 
        {
            TransformSystem.Unregister(this);
        }

        public Vector2 Up { get { return NormalizeVector(new Vector2(MathF.Cos(Rotation * MathF.PI / 180), MathF.Sin(Rotation * MathF.PI / 180))); } }
        public Vector2 Right { get { return NormalizeVector(new Vector2(MathF.Cos((Rotation - 90) * MathF.PI / 180), MathF.Sin((Rotation - 90) * MathF.PI / 180))); } }

        public Matrix4x4 GetPositionMatrix()
        {
            // Calculations below ensure that the position will scale with the screen width
            return Matrix4x4.CreateTranslation(Position.X * Display.WindowScale, Position.Y * Display.WindowScale, 0);
            //return Matrix4x4.CreateTranslation(position.X, position.Y, 0);
        }

        public Matrix4x4 GetScaleMatrix()
        {
            // Calculations below ensure that the scale will remain constant between screen sizes
            return Matrix4x4.CreateScale((Scale.X * Display.WindowScale) / 2f, (Scale.Y * Display.WindowScale) / 2f, 1);
            //return Matrix4x4.CreateScale(scale.X, scale.Y, 1);
        }

        public Matrix4x4 GetRotationMatrix()
        {
            // Converting from degrees to radians
            return Matrix4x4.CreateRotationZ(Rotation * (MathF.PI / 180));
        }

        private Vector2 NormalizeVector(Vector2 _vector)
        {
            float length = MathF.Sqrt(_vector.X * _vector.X + _vector.Y * _vector.Y);
            return _vector / length;
        }
    }
}
