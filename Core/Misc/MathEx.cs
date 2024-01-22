using System.Numerics;
using System;

namespace Electron2D.Core
{
    public static class MathEx
    {
        public static float Clamp(float _value, float _min, float _max)
        {
            return MathF.Max(0, MathF.Min(_value, _max));
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
            return _random.Next((int)(_min * 100000), (int)(_max * 100000) + 1) / 100000;
        }
    }
}