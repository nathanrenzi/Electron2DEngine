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
    }
}
