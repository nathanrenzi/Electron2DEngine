using System.Numerics;

namespace Electron2D
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

        public static float Lerp(float a, float b, float t)
        {
            t = Math.Clamp(t, 0, 1);
            return a + (b - a) * t;
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

        public static Vector2 RandomPositionInsideCircle(Random _random, float _radius)
        {
            var angle = _random.NextDouble() * Math.PI * 2;
            var distance = Math.Sqrt(_random.NextDouble()) * _radius;
            return new Vector2((float)(distance * Math.Cos(angle)), (float)(distance * Math.Sin(angle)));
        }

        public static Vector2 RandomPositionOnCircle(Random _random, float _radius)
        {
            var angle = _random.NextDouble() * Math.PI * 2;
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * _radius;
        }
    }
}