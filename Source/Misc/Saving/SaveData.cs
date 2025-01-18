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
    }
}
