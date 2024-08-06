using System.Numerics;
using System;

namespace Electron2D.Core
{
    public static class MathEx
    {
        public static float Clamp(float _value, float _min, float _max)
        {
            return MathF.Max(_min, MathF.Min(_value, _max));
        }

        public static float Clamp01(float _value)
        {
            return Clamp(_value, 0, 1);
        }

        public static Vector2 RotateVector2(Vector2 _vector2, float _degrees)
        {
            float radians = _degrees * (MathF.PI / 180f);

            return new Vector2(
            _vector2.X * MathF.Cos(radians) - _vector2.Y * MathF.Sin(radians),
                _vector2.X * MathF.Sin(radians) + _vector2.Y * MathF.Cos(radians)
            );
        }

        public static float RandomFloatInRange(Random _random, float _min, float _max)
        {
            float diff = _max - _min;
            float amount = diff * ((float)_random.NextDouble() + float.Epsilon);
            return _min + amount;
        }
    }
}