using System.Globalization;
using System.Windows.Media;

namespace Cell.Core.Common
{
    /// <summary>
    /// Contains helpers for converting and manipulating colors.
    /// </summary>
    public class ColorAdjuster
    {
        /// <summary>
        /// Adjusts the brightness of the given color by the given amount.
        /// </summary>
        /// <param name="hexColor">The color to adjust the brightness of.</param>
        /// <param name="brightnessFactor">The amount to ajust it by (0.5 would make the color darker, and 1.5 would make it lighter.)</param>
        /// <returns></returns>
        public static string AdjustBrightness(string hexColor, float brightnessFactor)
        {
            HexColorToHSL(hexColor, out float hue, out float saturation, out float lightness);
            lightness = AdjustLightness(brightnessFactor, lightness);
            return HSLToHexColor(hue, saturation, lightness);
        }

        /// <summary>
        /// Converts a Media.Color object to a hex color string.
        /// </summary>
        /// <param name="color">The Media.Color object.</param>
        /// <returns>A hex representation of the color.</returns>
        public static string ConvertColorToHexString(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Converts a hex color string to a SolidColorBrush.
        /// </summary>
        /// <param name="hex">The hex string.</param>
        /// <returns>A <see cref="SolidColorBrush"/>.</returns>
        public static SolidColorBrush ConvertHexStringToBrush(string hex)
        {
            var color = ConvertHexStringToColor(hex);
            return new SolidColorBrush(color);
        }

        /// <summary>
        /// Converts a hex color string to a Media.Color object.
        /// </summary>
        /// <param name="hex">The hex string.</param>
        /// <returns>A Media.Color object.</returns>
        public static Color ConvertHexStringToColor(string hex)
        {
            if (!hex.StartsWith('#') || hex.Length != 7) return Colors.Green;
            try
            {
                byte r = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
                return Color.FromRgb(r, g, b);
            }
            catch (FormatException) { }
            catch (ArgumentException) { }
            return Colors.Green;
        }

        /// <summary>
        /// Gets a highlight color that contrasts well with the given background color.
        /// </summary>
        /// <param name="backgroundColor">The background color.</param>
        /// <param name="alpha">The amount of transparency to add.</param>
        /// <returns>A highlight color with good contrast against the background color.</returns>
        public static Color GetHighlightColor(Color backgroundColor, byte alpha)
        {
            // Calculate the luminance of the background color
            double luminance = 0.2126 * backgroundColor.R / 255.0 +
                               0.7152 * backgroundColor.G / 255.0 +
                               0.0722 * backgroundColor.B / 255.0;

            // Determine whether to use a light or dark highlight color based on luminance
            if (luminance > 0.5)
            {
                // Background is light, use a darker highlight color
                return Color.FromArgb(alpha, 0, 0, 0); // Black
            }
            else
            {
                // Background is dark, use a lighter highlight color
                return Color.FromArgb(alpha, 255, 255, 255); // White
            }
        }

        private static float AdjustLightness(float brightnessFactor, float lightness)
        {
            brightnessFactor = Math.Max(0f, brightnessFactor);
            var adjustedBrightness = lightness * brightnessFactor;
            return Math.Max(0f, Math.Min(1f, adjustedBrightness));
        }

        private static void ColorToHSL(float red, float green, float blue, out float hue, out float saturation, out float lightness)
        {
            var max = Math.Max(red, Math.Max(green, blue));
            var min = Math.Min(red, Math.Min(green, blue));
            var delta = max - min;
            lightness = GetLightness(max, min);
            saturation = GetSaturation(lightness, max, min, delta);
            hue = GetHue(red, green, blue, max, delta);
        }

        private static void ColorToHSL(int red, int green, int blue, out float hue, out float saturation, out float lightness)
        {
            var redPercent = red / 255f;
            var greenPercent = green / 255f;
            var bluePercent = blue / 255f;
            ColorToHSL(redPercent, greenPercent, bluePercent, out hue, out saturation, out lightness);
        }

        private static float GetHue(float redPercent, float greenPercent, float bluePercent, float max, float delta)
        {
            if (delta == 0) return 0;
            float hue;
            if (max == redPercent) hue = (greenPercent - bluePercent) / delta + (greenPercent < bluePercent ? 6f : 0f);
            else if (max == greenPercent) hue = (bluePercent - redPercent) / delta + 2f;
            else hue = (redPercent - greenPercent) / delta + 4f;
            hue /= 6f;
            return hue;
        }

        private static float GetLightness(float max, float min) => (max + min) / 2f;

        private static float GetSaturation(float lightness, float max, float min, float delta)
        {
            if (delta == 0) return 0;
            return (lightness > 0.5f)
                ? delta / (2f - max - min)
                : delta / (max + min);
        }

        private static void HexColorToHSL(string hexColor, out float hue, out float saturation, out float lightness)
        {
            if (hexColor.StartsWith('#')) hexColor = hexColor[1..];
            int red = Convert.ToInt32(hexColor[..2], 16);
            int green = Convert.ToInt32(hexColor.Substring(2, 2), 16);
            int blue = Convert.ToInt32(hexColor.Substring(4, 2), 16);
            ColorToHSL(red, green, blue, out hue, out saturation, out lightness);
        }

        private static string HSLToHexColor(float hue, float saturation, float lightness)
        {
            HSLToRGBColor(hue, saturation, lightness, out int newR, out int newG, out int newB);
            return $"#{newR:X2}{newG:X2}{newB:X2}";
        }

        private static void HSLToRGBColor(float hue, float saturation, float lightness, out int red, out int green, out int blue)
        {
            float temp1, temp2;
            if (saturation == 0) red = green = blue = (int)(lightness * 255f);
            else
            {
                temp2 = (lightness < 0.5f) ? (lightness * (1f + saturation)) : (lightness + saturation - lightness * saturation);
                temp1 = 2f * lightness - temp2;

                red = (int)(255f * HueToRGB(temp1, temp2, hue + 1f / 3f));
                green = (int)(255f * HueToRGB(temp1, temp2, hue));
                blue = (int)(255f * HueToRGB(temp1, temp2, hue - 1f / 3f));
            }
        }

        private static float HueToRGB(float temp1, float temp2, float hue)
        {
            if (hue < 0f) hue += 1f;
            if (hue > 1f) hue -= 1f;

            if (6f * hue < 1f) return temp1 + (temp2 - temp1) * 6f * hue;
            if (2f * hue < 1f) return temp2;
            if (3f * hue < 2f) return temp1 + (temp2 - temp1) * (2f / 3f - hue) * 6f;
            return temp1;
        }
    }
}
