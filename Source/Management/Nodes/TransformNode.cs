using System.Numerics;

namespace Atlas2D
{
    public class TransformNode : Node
    {
        public event Action OnTransformChanged;

        private Vector2 _localPosition;
        private Vector2 _localScale = Vector2.One;
        private float _localRotation;

        public Vector2 LocalPosition
        {
            get => _localPosition;
            set { _localPosition = value; OnTransformChanged?.Invoke(); }
        }

        public Vector2 LocalScale
        {
            get => _localScale;
            set { _localScale = value; OnTransformChanged?.Invoke(); }
        }

        public float LocalRotation
        {
            get => _localRotation;
            set { _localRotation = value; OnTransformChanged?.Invoke(); }
        }

        public Vector2 WorldPosition
        {
            get
            {
                var parent = GetParentTransformNode();
                if (parent == null) return LocalPosition;

                float rad = parent.WorldRotation * MathF.PI / 180f;
                Vector2 scaled = LocalPosition * parent.WorldScale;
                Vector2 rotated = new(
                    scaled.X * MathF.Cos(rad) - scaled.Y * MathF.Sin(rad),
                    scaled.X * MathF.Sin(rad) + scaled.Y * MathF.Cos(rad)
                );
                return parent.WorldPosition + rotated;
            }
        }

        public Vector2 WorldScale
        {
            get
            {
                var parent = GetParentTransformNode();
                return parent != null ? LocalScale * parent.WorldScale : LocalScale;
            }
        }

        public float WorldRotation
        {
            get
            {
                var parent = GetParentTransformNode();
                return LocalRotation + (parent?.WorldRotation ?? 0f);
            }
        }

        public Matrix4x4 LocalMatrix =>
            Matrix4x4.CreateScale(LocalScale.X, LocalScale.Y, 1f) *
            Matrix4x4.CreateRotationZ(LocalRotation * MathF.PI / 180f) *
            Matrix4x4.CreateTranslation(LocalPosition.X, LocalPosition.Y, 0f);

        public Matrix4x4 WorldMatrix
        {
            get
            {
                var parent = GetParentTransformNode();
                return parent != null ? LocalMatrix * parent.WorldMatrix : LocalMatrix;
            }
        }

        private float WorldRadians => WorldRotation * MathF.PI / 180f;
        public Vector2 Up => new(MathF.Sin(WorldRadians), -MathF.Cos(WorldRadians));
        public Vector2 Right => new(MathF.Cos(WorldRadians), MathF.Sin(WorldRadians));

        private TransformNode? GetParentTransformNode()
        {
            var current = Parent;
            while (current != null)
            {
                if (current is TransformNode tn) return tn;
                current = current.Parent;
            }
            return null;
        }
    }
}