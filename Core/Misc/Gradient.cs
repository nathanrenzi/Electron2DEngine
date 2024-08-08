using System.Drawing;

namespace Electron2D.Core.Misc
{
    public class Gradient
    {
        public SortedList<float, Color> ColorsSorted { get; } = new SortedList<float, Color>();

        public Gradient() { }
        public Gradient(Color _color)
        {
            Add(_color, 0);
        }
        public Gradient(Color[] _colors)
        {
            float interval = 1f / MathF.Max(_colors.Length - 1, 1);
            float currentInterval = 0;
            foreach (var color in _colors)
            {
                Add(color, currentInterval);
                currentInterval += interval;
            }
        }

        public bool Add(Color _color, float _position)
        {
            bool success = ColorsSorted.TryAdd(_position, _color);
            return success;
        }

        public Color Evaluate(float _percentage)
        {
            if (ColorsSorted.Count == 0)
            {
                return Color.Black;
            }
            else if (ColorsSorted.Count == 1)
            {
                return ColorsSorted.Values[0];
            }

            // Edge cases
            if (_percentage <= ColorsSorted.Keys[0])
            {
                return ColorsSorted.Values[0];
            }
            else if (_percentage >= ColorsSorted.Keys[ColorsSorted.Keys.Count - 1])
            {
                return ColorsSorted.Values[ColorsSorted.Values.Count - 1];
            }

            for (int i = 0; i < ColorsSorted.Keys.Count - 1; i++)
            {
                if (ColorsSorted.Keys[i] <= _percentage && ColorsSorted.Keys[i + 1] >= _percentage)
                {
                    Color c1 = ColorsSorted.Values[i];
                    Color c2 = ColorsSorted.Values[i + 1];
                    float p1 = ColorsSorted.Keys[i];
                    float p2 = ColorsSorted.Keys[i + 1];

                    return ColorInterp(c1, c2, (_percentage - p1) / (p2 - p1));
                }
            }

            return Color.Black;
        }

        private static int LinearInterp(int _start, int _end, float _percentage) => _start + (int)Math.Round(_percentage * (_end - _start));
        public static Color ColorInterp(Color _start, Color _end, float _percentage) =>
            Color.FromArgb(LinearInterp(_start.A, _end.A, _percentage),
                           LinearInterp(_start.R, _end.R, _percentage),
                           LinearInterp(_start.G, _end.G, _percentage),
                           LinearInterp(_start.B, _end.B, _percentage));
    }
}