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
        public Vector2 Position;
        public Vector2 Scale = new Vector2(100, 100);
        public Vector2 PivotPoint;
        public float Rotation;
        public bool IsDirty { get; private set; } = true;

        // Checking for changed values
        private Vector2 lastPosition;
        private Vector2 lastScale;
        private Vector2 lastPivotPoint;
        private float lastRotation;
        // ---------------------------

        public Transform()
        {
            PivotPoint = Vector2.Zero;
            Scale = Vector2.One;

            Game.onLateUpdateEvent += CheckForNewValues;
        }

        public Vector2 up { get { return NormalizeVector(new Vector2(MathF.Cos(Rotation * MathF.PI / 180), MathF.Sin(Rotation * MathF.PI / 180))); } }
        public Vector2 right { get { return NormalizeVector(new Vector2(MathF.Cos((Rotation - 90) * MathF.PI / 180), MathF.Sin((Rotation - 90) * MathF.PI / 180))); } }

        /// <summary>
        /// Checks to see if values have been updated, and if so mark the transform as dirty.
        /// </summary>
        private void CheckForNewValues()
        {
            if(lastPosition != Position ||
                lastScale != Scale ||
                lastPivotPoint != PivotPoint ||
                lastRotation != Rotation)
            {
                IsDirty = true;
            }

            lastPosition = Position;
            lastScale = Scale;
            lastPivotPoint = PivotPoint;
            lastRotation = Rotation;
        }

        public Matrix4x4 GetPositionMatrix()
        {
            // Calculations below ensure that the position will scale with the screen width
            return Matrix4x4.CreateTranslation(Position.X * Game.WINDOW_SCALE, Position.Y * Game.WINDOW_SCALE, 0);
            //return Matrix4x4.CreateTranslation(position.X, position.Y, 0);
        }

        public Matrix4x4 GetScaleMatrix()
        {
            // Calculations below ensure that the scale will remain constant between screen sizes
            return Matrix4x4.CreateScale((Scale.X * Game.WINDOW_SCALE) / 2f, (Scale.Y * Game.WINDOW_SCALE) / 2f, 1);
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

        public void UnflagDirty() => IsDirty = false;
    }
}
