using System.Drawing;

namespace Electron2D
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a 6-char HEX code into Color data.
        /// </summary>
        /// <param name="_color"></param>
        /// <param name="_hex">6-char HEX code.</param>
        /// <param name="_alpha">The alpha of the color. 0-255.</param>
        /// <returns></returns>
        public static Color HexToColor(this string _hex, int _alpha)
        {
            int r = int.Parse(_hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(_hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(_hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            return Color.FromArgb(_alpha, r, g, b);
        }
    }
}
