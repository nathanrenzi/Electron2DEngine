using System.Numerics;

namespace Electron2D
{
    public class SaveData
    {
        public List<string> Strings { get; set; } = new List<string>();
        public List<float> Floats { get; set; } = new List<float>();
        public List<int> Ints { get; set; } = new List<int>();
        public List<bool> Bools { get; set; } = new List<bool>();
        public List<byte> Bytes { get; set; } = new List<byte>();
        public List<Vector2> Vectors { get; set; } = new List<Vector2>();

        public string? GetString()
        {
            if (Strings.Count > 0)
            {
                string retVal = Strings[0];
                Strings.RemoveAt(0);
                return retVal;
            }
            else
            {
                return null;
            }
        }

        public float? GetFloat()
        {
            if (Floats.Count > 0)
            {
                float retVal = Floats[0];
                Floats.RemoveAt(0);
                return retVal;
            }
            else
            {
                return null;
            }
        }

        public int? GetInt()
        {
            if (Ints.Count > 0)
            {
                int retVal = Ints[0];
                Ints.RemoveAt(0);
                return retVal;
            }
            else
            {
                return null;
            }
        }

        public bool? GetBool()
        {
            if (Bools.Count > 0)
            {
                bool retVal = Bools[0];
                Bools.RemoveAt(0);
                return retVal;
            }
            else
            {
                return null;
            }
        }

        public byte? GetByte()
        {
            if (Bytes.Count > 0)
            {
                byte retVal = Bytes[0];
                Bytes.RemoveAt(0);
                return retVal;
            }
            else
            {
                return null;
            }
        }

        public Vector2? GetVector2()
        {
            if (Vectors.Count > 0)
            {
                Vector2 retVal = Vectors[0];
                Vectors.RemoveAt(0);
                return retVal;
            }
            else
            {
                return null;
            }
        }
    }
}
