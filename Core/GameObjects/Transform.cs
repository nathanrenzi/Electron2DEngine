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
        public Vector2 position;
        public Vector2 scale = new Vector2(100, 100);
        public Vector2 pivotPoint;
        public float rotation;
        public bool isDirty { get; private set; } = true;

        // Checking for changed values
        private Vector2 lastPosition;
        private Vector2 lastScale;
        private Vector2 lastPivotPoint;
        public float lastRotation;
        // ---------------------------

        public Transform()
        {
            pivotPoint = Vector2.Zero;
            scale = Vector2.One;

            Game.onLateUpdateEvent += CheckForNewValues;
        }

        public Vector2 up { get { return NormalizeVector(new Vector2(MathF.Cos(rotation * MathF.PI / 180), MathF.Sin(rotation * MathF.PI / 180))); } }
        public Vector2 right { get { return NormalizeVector(new Vector2(MathF.Cos((rotation - 90) * MathF.PI / 180), MathF.Sin((rotation - 90) * MathF.PI / 180))); } }

        /// <summary>
        /// Checks to see if values have been updated, and if so mark the transform as dirty.
        /// </summary>
        private void CheckForNewValues()
        {
            if(lastPosition != position ||
                lastScale != scale ||
                lastPivotPoint != pivotPoint ||
                lastRotation != rotation)
            {
                isDirty = true;
            }

            lastPosition = position;
            lastScale = scale;
            lastPivotPoint = pivotPoint;
            lastRotation = rotation;
        }

        public Matrix4x4 GetPositionMatrix()
        {
            // Calculations below ensure that the position will scale with the screen width
            return Matrix4x4.CreateTranslation(position.X * Game.WINDOW_SCALE, position.Y * Game.WINDOW_SCALE, 0);
            //return Matrix4x4.CreateTranslation(position.X, position.Y, 0);
        }

        public Matrix4x4 GetScaleMatrix()
        {
            // Calculations below ensure that the scale will remain constant between screen sizes
            return Matrix4x4.CreateScale((scale.X * Game.WINDOW_SCALE) / 2f, (scale.Y * Game.WINDOW_SCALE) / 2f, 1);
            //return Matrix4x4.CreateScale(scale.X, scale.Y, 1);
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

        public void UnflagDirty() => isDirty = false;
    }
}
