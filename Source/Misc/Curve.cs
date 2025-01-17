using System.Numerics;
using System.Text;

namespace Electron2D
{
    public class Curve
    {
        public List<Point> Points = new List<Point>();

        public Curve()
        {
            // Start and end points
            Points.Add(new Point(0, 0, Handle.Left, Handle.Right));
            Points.Add(new Point(1, 1, Handle.Left, Handle.Right));
        }

        public Curve(string _hash)
        {
            try
            {
                int index = _hash.IndexOf('=') + 1;
                int index2 = 0;
                for (int i = index; i < _hash.Length; i++)
                {
                    if (_hash[i] == '=')
                    {
                        index2 = i;
                        break;
                    }
                }
                int length = index2 - index;
                string hash = _hash.Substring(index, length);
                string[] pointHashes = hash.Split('_');
                string[] valueStrings;
                for (int i = 0; i < pointHashes.Length; i++)
                {
                    valueStrings = pointHashes[i].Split(':');
                    float time, value, lht, lhv, rht, rhv;
                    time = float.Parse(valueStrings[0]);
                    value = float.Parse(valueStrings[1]);
                    lht = float.Parse(valueStrings[2]);
                    lhv = float.Parse(valueStrings[3]);
                    rht = float.Parse(valueStrings[4]);
                    rhv = float.Parse(valueStrings[5]);
                    Points.Add(new Point(time, value, new Handle(lht, lhv), new Handle(rht, rhv)));
                }
            }
            catch(Exception e)
            {
                Debug.LogError("Error pasting curve from clipboard");
                Debug.LogError(e.ToString());
            }
        }

        public Curve(List<Point> _points)
        {
            for (int i = 0; i < _points.Count; i++)
            {
                Points.Add(_points[i]);
            }
        }

        public int AddPoint(Point _point)
        {
            if(_point.Time < 0 || _point.Time > 1)
            {
                Debug.LogError("Time value of Curve.Point cannot be less than zero or greater than one!");
                return 0;
            }

            for (int i = 0; i < Points.Count - 1; i++)
            {
                if (Points[i].Time < _point.Time && _point.Time < Points[i + 1].Time)
                {
                    Points.Insert(i + 1, _point);
                    return i + 1;
                }
            }

            return 0;
        }

        public void RemovePoint(Point _point)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0 || i == Points.Count - 1) continue;
                if (Points[i] == _point)
                {
                    Points.RemoveAt(i);
                }
            }
        }

        public float Evaluate(float _time)
        {
            _time = MathEx.Clamp01(_time);

            for (int i = 0; i < Points.Count - 1; i++)
            {
                Point p = Points[i];
                Point pp = Points[i + 1];

                if (p.Time <= _time && pp.Time >= _time)
                {
                    float d = pp.Time - p.Time;
                    float t = (_time - p.Time) / d;

                    return CubicBezierCurve(new Vector2(p.Time, p.Value), new Vector2(p.Time + p.RightHandle.RelativeTime, p.Value + p.RightHandle.RelativeValue),
                        new Vector2(pp.Time, pp.Value), new Vector2(pp.Time + pp.LeftHandle.RelativeTime, pp.Value + pp.LeftHandle.RelativeValue), t).Y;
                }
            }

            Debug.LogError("Invalid bezier curve points.");
            return 0;
        }

        //https://en.wikipedia.org/wiki/B%C3%A9zier_curve#:~:text=.-,Cubic%20B%C3%A9zier%20curves,-%5Bedit%5D
        private Vector2 CubicBezierCurve(Vector2 _p0, Vector2 _p1, Vector2 _p2, Vector2 _p3, float _t)
        {
            return MathF.Pow(1 - _t, 3) * _p0 + 3 * MathF.Pow(1 - _t, 2) * _t * _p1 + 3 * (1 - _t) * MathF.Pow(_t, 2) * _p2 + MathF.Pow(_t, 3) * _p3;
        }

        public string ToHash()
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < Points.Count; i++)
            {
                builder.Append(Points[i].ToHash());
                if(i < Points.Count - 1) builder.Append("_");
            }

            return builder.ToString();
        }

        public struct Point
        {
            public float Time;
            public float Value;
            public Handle LeftHandle = new Handle();
            public Handle RightHandle = new Handle();

            public Point(float _time, float _value, Handle _leftHandle, Handle _rightHandle)
            {
                Time = _time;
                Value = _value;
                LeftHandle = _leftHandle;
                RightHandle = _rightHandle;
            }

            public string ToCode()
            {
                return "new Curve.Point("+Time+"f, "+Value+"f, "+LeftHandle.ToCode()+", "+RightHandle.ToCode()+")";
            }

            public string ToHash()
            {
                return Time+":"+Value+":"+LeftHandle.RelativeTime+":"+LeftHandle.RelativeValue+":"+RightHandle.RelativeTime+":"+RightHandle.RelativeValue;
            }

            public static bool operator ==(Point p1, Point p2)
            {
                return (p1.Time == p2.Time
                    && p1.Value == p2.Value
                    && p1.LeftHandle.RelativeTime == p2.LeftHandle.RelativeTime
                    && p1.LeftHandle.RelativeValue == p2.LeftHandle.RelativeValue
                    && p1.RightHandle.RelativeTime == p2.RightHandle.RelativeTime
                    && p1.RightHandle.RelativeValue == p2.RightHandle.RelativeValue);
            }

            public static bool operator !=(Point p1, Point p2)
            {
                return !(p1.Time == p2.Time
                    && p1.Value == p2.Value
                    && p1.LeftHandle.RelativeTime == p2.LeftHandle.RelativeTime
                    && p1.LeftHandle.RelativeValue == p2.LeftHandle.RelativeValue
                    && p1.RightHandle.RelativeTime == p2.RightHandle.RelativeTime
                    && p1.RightHandle.RelativeValue == p2.RightHandle.RelativeValue);
            }
        }

        public struct Handle
        {
            public float RelativeTime;
            public float RelativeValue;

            public Handle(float _relativeTime, float _relativeValue)
            {
                RelativeTime = _relativeTime;
                RelativeValue = _relativeValue;
            }

            public string ToCode()
            {
                return "new Curve.Handle("+RelativeTime+"f, "+RelativeValue+"f)";
            }

            public static Handle Left = new Handle(-0.5f, 0);

            public static Handle Right = new Handle(0.5f, 0);
        }
    }
}
