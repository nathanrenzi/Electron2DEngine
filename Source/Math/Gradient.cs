namespace Electron2D
{
    public struct Gradient
    {
        public SortedList<float, Color> ColorsSorted { get; } = new SortedList<float, Color>();

        public Gradient() { }

        public Gradient(Color color)
        {
            Add(color, 0);
        }

        public Gradient(Color[] colors)
        {
            float interval = 1f / MathF.Max(colors.Length - 1, 1);
            float currentInterval = 0;
            foreach (var color in colors)
            {
                Add(color, currentInterval);
                currentInterval += interval;
            }
        }

        public bool Add(Color color, float position)
        {
            return ColorsSorted.TryAdd(position, color);
        }

        public Color Evaluate(float percentage)
        {
            if (ColorsSorted.Count == 0)
            {
                return Color.Black;
            }
            else if (ColorsSorted.Count == 1)
            {
                return ColorsSorted.Values[0];
            }

            if (percentage <= ColorsSorted.Keys[0])
            {
                return ColorsSorted.Values[0];
            }
            else if (percentage >= ColorsSorted.Keys[ColorsSorted.Keys.Count - 1])
            {
                return ColorsSorted.Values[ColorsSorted.Values.Count - 1];
            }

            for (int i = 0; i < ColorsSorted.Keys.Count - 1; i++)
            {
                if (ColorsSorted.Keys[i] <= percentage && ColorsSorted.Keys[i + 1] >= percentage)
                {
                    Color c1 = ColorsSorted.Values[i];
                    Color c2 = ColorsSorted.Values[i + 1];
                    float p1 = ColorsSorted.Keys[i];
                    float p2 = ColorsSorted.Keys[i + 1];

                    return Color.Lerp(c1, c2, (percentage - p1) / (p2 - p1));
                }
            }

            return Color.Black;
        }
    }
}