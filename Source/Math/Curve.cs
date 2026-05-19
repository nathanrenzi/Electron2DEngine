using System.Globalization;
using System.Numerics;
using System.Text;

namespace Atlas2D
{
    public struct Curve
    {
        public List<Point> Points = new List<Point>();

        public Curve()
        {
            Points.Add(new Point(0, 0, Handle.Left, Handle.Right));
            Points.Add(new Point(1, 1, Handle.Left, Handle.Right));
        }

        public Curve(string hash)
        {
            try
            {
                // Strip optional "name=" prefix if present
                int eq = hash.IndexOf('=');
                string body = eq >= 0 ? hash[(eq + 1)..] : hash;
                // Strip optional trailing '=' delimiter
                int trailingEq = body.IndexOf('=');
                if (trailingEq >= 0) body = body[..trailingEq];

                string[] pointHashes = body.Split('_');
                for (int i = 0; i < pointHashes.Length; i++)
                {
                    string[] v = pointHashes[i].Split(':');
                    var inv = CultureInfo.InvariantCulture;
                    float time = float.Parse(v[0], inv);
                    float value = float.Parse(v[1], inv);
                    float lht = float.Parse(v[2], inv);
                    float lhv = float.Parse(v[3], inv);
                    float rht = float.Parse(v[4], inv);
                    float rhv = float.Parse(v[5], inv);
                    Points.Add(new Point(time, value, new Handle(lht, lhv), new Handle(rht, rhv)));
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error pasting curve from clipboard");
                Debug.LogError(e.ToString());
            }
        }

        public Curve(List<Point> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Points.Add(points[i]);
            }
        }

        public int AddPoint(Point point)
        {
            if (point.Time < 0 || point.Time > 1)
            {
                Debug.LogError("Time value of Curve.Point cannot be less than zero or greater than one!");
                return -1;
            }

            for (int i = 0; i < Points.Count - 1; i++)
            {
                if (Points[i].Time < point.Time && point.Time < Points[i + 1].Time)
                {
                    Points.Insert(i + 1, point);
                    return i + 1;
                }
            }

            return -1;
        }

        public void RemovePoint(Point point)
        {
            // Skip endpoints — they're not removable
            for (int i = 1; i < Points.Count - 1; i++)
            {
                if (Points[i] == point)
                {
                    Points.RemoveAt(i);
                    return;
                }
            }
        }

        public float Evaluate(float time)
        {
            time = MathEx.Clamp01(time);

            for (int i = 0; i < Points.Count - 1; i++)
            {
                Point p = Points[i];
                Point pp = Points[i + 1];

                if (p.Time <= time && pp.Time >= time)
                {
                    float d = pp.Time - p.Time;
                    float t = d > 0 ? (time - p.Time) / d : 0;

                    return CubicBezierCurve(
                        new Vector2(p.Time, p.Value),
                        new Vector2(p.Time + p.RightHandle.RelativeTime, p.Value + p.RightHandle.RelativeValue),
                        new Vector2(pp.Time + pp.LeftHandle.RelativeTime, pp.Value + pp.LeftHandle.RelativeValue),
                        new Vector2(pp.Time, pp.Value),
                        t).Y;
                }
            }

            Debug.LogError("Invalid bezier curve points.");
            return 0;
        }

        // https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Cubic_B%C3%A9zier_curves
        private static Vector2 CubicBezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            float uu = u * u;
            float uuu = uu * u;
            float tt = t * t;
            float ttt = tt * t;
            return uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
        }

        public string ToHash()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < Points.Count; i++)
            {
                builder.Append(Points[i].ToHash());
                if (i < Points.Count - 1) builder.Append('_');
            }
            return builder.ToString();
        }

        public struct Point : IEquatable<Point>
        {
            public float Time;
            public float Value;
            public Handle LeftHandle;
            public Handle RightHandle;

            public Point(float time, float value, Handle leftHandle, Handle rightHandle)
            {
                Time = time;
                Value = value;
                LeftHandle = leftHandle;
                RightHandle = rightHandle;
            }

            public string ToCode()
            {
                var inv = CultureInfo.InvariantCulture;
                return $"new Curve.Point({Time.ToString(inv)}f, {Value.ToString(inv)}f, {LeftHandle.ToCode()}, {RightHandle.ToCode()})";
            }

            public string ToHash()
            {
                var inv = CultureInfo.InvariantCulture;
                return string.Create(inv, $"{Time}:{Value}:{LeftHandle.RelativeTime}:{LeftHandle.RelativeValue}:{RightHandle.RelativeTime}:{RightHandle.RelativeValue}");
            }

            public bool Equals(Point other)
                => Time == other.Time
                && Value == other.Value
                && LeftHandle.RelativeTime == other.LeftHandle.RelativeTime
                && LeftHandle.RelativeValue == other.LeftHandle.RelativeValue
                && RightHandle.RelativeTime == other.RightHandle.RelativeTime
                && RightHandle.RelativeValue == other.RightHandle.RelativeValue;

            public override bool Equals(object? obj) => obj is Point p && Equals(p);
            public override int GetHashCode() => HashCode.Combine(Time, Value, LeftHandle.RelativeTime, LeftHandle.RelativeValue, RightHandle.RelativeTime, RightHandle.RelativeValue);
            public static bool operator ==(Point p1, Point p2) => p1.Equals(p2);
            public static bool operator !=(Point p1, Point p2) => !p1.Equals(p2);
        }

        public struct Handle
        {
            public float RelativeTime;
            public float RelativeValue;

            public Handle(float relativeTime, float relativeValue)
            {
                RelativeTime = relativeTime;
                RelativeValue = relativeValue;
            }

            public string ToCode()
            {
                var inv = CultureInfo.InvariantCulture;
                return $"new Curve.Handle({RelativeTime.ToString(inv)}f, {RelativeValue.ToString(inv)}f)";
            }

            public static Handle Left => new Handle(-0.5f, 0);
            public static Handle Right => new Handle(0.5f, 0);
        }
    }
}