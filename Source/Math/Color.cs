using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Atlas2D
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color : IEquatable<Color>
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly float A;

        public Color(float r, float g, float b, float a = 1f)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public static Color FromBytes(byte r, byte g, byte b, byte a = 255)
            => new(r / 255f, g / 255f, b / 255f, a / 255f);

        public static Color FromRgba(uint rgba)
            => FromBytes(
                (byte)(rgba >> 24),
                (byte)(rgba >> 16),
                (byte)(rgba >> 8),
                (byte)rgba);

        public static Color FromArgb(uint argb)
            => FromBytes(
                (byte)(argb >> 16),
                (byte)(argb >> 8),
                (byte)argb,
                (byte)(argb >> 24));

        public uint ToRgba()
            => ((uint)ToByte(R) << 24)
             | ((uint)ToByte(G) << 16)
             | ((uint)ToByte(B) << 8)
             | (uint)ToByte(A);

        public uint ToArgb()
            => ((uint)ToByte(A) << 24)
             | ((uint)ToByte(R) << 16)
             | ((uint)ToByte(G) << 8)
             | (uint)ToByte(B);

        public byte RByte => ToByte(R);
        public byte GByte => ToByte(G);
        public byte BByte => ToByte(B);
        public byte AByte => ToByte(A);

        public Color WithAlpha(float a) => new(R, G, B, a);

        public static Color FromHex(ReadOnlySpan<char> hex, float alpha = 1f)
        {
            if (hex.Length > 0 && hex[0] == '#')
                hex = hex[1..];

            switch (hex.Length)
            {
                case 3:
                    {
                        byte r = ParseHexDigit(hex[0]);
                        byte g = ParseHexDigit(hex[1]);
                        byte b = ParseHexDigit(hex[2]);
                        return FromBytes(
                            (byte)(r << 4 | r),
                            (byte)(g << 4 | g),
                            (byte)(b << 4 | b),
                            ToByte(alpha));
                    }
                case 6:
                    {
                        byte r = ParseHexByte(hex[..2]);
                        byte g = ParseHexByte(hex.Slice(2, 2));
                        byte b = ParseHexByte(hex.Slice(4, 2));
                        return FromBytes(r, g, b, ToByte(alpha));
                    }
                case 8:
                    {
                        byte r = ParseHexByte(hex[..2]);
                        byte g = ParseHexByte(hex.Slice(2, 2));
                        byte b = ParseHexByte(hex.Slice(4, 2));
                        byte a = ParseHexByte(hex.Slice(6, 2));
                        return FromBytes(r, g, b, a);
                    }
                default:
                    throw new FormatException(
                        $"Hex color must be 3, 6, or 8 characters (got {hex.Length}).");
            }
        }

        public static Color FromHex(string hex, float alpha = 1f)
            => FromHex(hex.AsSpan(), alpha);

        public string ToHex(bool includeAlpha = false)
            => includeAlpha
                ? $"#{ToByte(R):X2}{ToByte(G):X2}{ToByte(B):X2}{ToByte(A):X2}"
                : $"#{ToByte(R):X2}{ToByte(G):X2}{ToByte(B):X2}";

        private static byte ParseHexByte(ReadOnlySpan<char> s)
            => (byte)((ParseHexDigit(s[0]) << 4) | ParseHexDigit(s[1]));

        private static byte ParseHexDigit(char c) => c switch
        {
            >= '0' and <= '9' => (byte)(c - '0'),
            >= 'a' and <= 'f' => (byte)(c - 'a' + 10),
            >= 'A' and <= 'F' => (byte)(c - 'A' + 10),
            _ => throw new FormatException($"Invalid hex digit: '{c}'")
        };

        public static Color Lerp(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return new Color(
                a.R + (b.R - a.R) * t,
                a.G + (b.G - a.G) * t,
                a.B + (b.B - a.B) * t,
                a.A + (b.A - a.A) * t);
        }

        public static Color operator *(Color a, Color b)
            => new(a.R * b.R, a.G * b.G, a.B * b.B, a.A * b.A);

        public static Color operator *(Color c, float scalar)
            => new(c.R * scalar, c.G * scalar, c.B * scalar, c.A * scalar);

        public static Color operator +(Color a, Color b)
            => new(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);

        public Vector4 ToVector4() => new(R, G, B, A);

        public static implicit operator Vector4(Color c) => new(c.R, c.G, c.B, c.A);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte ToByte(float v) => (byte)(Math.Clamp(v, 0f, 1f) * 255f + 0.5f);

        public bool Equals(Color other)
            => R == other.R && G == other.G && B == other.B && A == other.A;
        public override bool Equals(object? obj) => obj is Color c && Equals(c);
        public override int GetHashCode() => HashCode.Combine(R, G, B, A);
        public static bool operator ==(Color a, Color b) => a.Equals(b);
        public static bool operator !=(Color a, Color b) => !a.Equals(b);

        public override string ToString() => $"Color(R={R:0.###}, G={G:0.###}, B={B:0.###}, A={A:0.###})";

        public static Color White => new(1f, 1f, 1f);
        public static Color Black => new(0f, 0f, 0f);
        public static Color Transparent => new(0f, 0f, 0f, 0f);
        public static Color Red => new(1f, 0f, 0f);
        public static Color Green => new(0f, 1f, 0f);
        public static Color Blue => new(0f, 0f, 1f);
    }
}