using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cell.View
{
    class ColorAdjuster
    {
        public static string AdjustBrightness(string hexColor, float brightnessFactor)
        {
            // Ensure brightnessFactor is within the valid range
            brightnessFactor = Math.Max(0f, brightnessFactor);

            // Remove the leading '#'
            if (hexColor.StartsWith("#"))
            {
                hexColor = hexColor.Substring(1);
            }

            // Parse the hex color into RGB components
            int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
            int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
            int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);

            // Convert RGB to HSL
            float h, s, l;
            ColorToHSL(r, g, b, out h, out s, out l);

            // Adjust the lightness
            l = Math.Max(0f, Math.Min(1f, l * brightnessFactor));

            // Convert HSL back to RGB
            int newR, newG, newB;
            HSLToColor(h, s, l, out newR, out newG, out newB);

            // Convert back to hex
            return $"#{newR:X2}{newG:X2}{newB:X2}";
        }

        private static void ColorToHSL(int r, int g, int b, out float h, out float s, out float l)
        {
            float rf = r / 255f;
            float gf = g / 255f;
            float bf = b / 255f;

            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            float delta = max - min;

            l = (max + min) / 2f;

            if (delta == 0)
            {
                h = s = 0;
            }
            else
            {
                s = (l > 0.5f) ? delta / (2f - max - min) : delta / (max + min);

                if (max == rf)
                    h = (gf - bf) / delta + (gf < bf ? 6f : 0f);
                else if (max == gf)
                    h = (bf - rf) / delta + 2f;
                else
                    h = (rf - gf) / delta + 4f;

                h /= 6f;
            }
        }

        private static void HSLToColor(float h, float s, float l, out int r, out int g, out int b)
        {
            float temp1, temp2;

            if (s == 0)
            {
                r = g = b = (int)(l * 255f);
            }
            else
            {
                temp2 = (l < 0.5f) ? (l * (1f + s)) : (l + s - l * s);
                temp1 = 2f * l - temp2;

                r = (int)(255f * HueToRGB(temp1, temp2, h + 1f / 3f));
                g = (int)(255f * HueToRGB(temp1, temp2, h));
                b = (int)(255f * HueToRGB(temp1, temp2, h - 1f / 3f));
            }
        }

        private static float HueToRGB(float temp1, float temp2, float hue)
        {
            if (hue < 0f)
                hue += 1f;
            if (hue > 1f)
                hue -= 1f;

            if (6f * hue < 1f)
                return temp1 + (temp2 - temp1) * 6f * hue;
            if (2f * hue < 1f)
                return temp2;
            if (3f * hue < 2f)
                return temp1 + (temp2 - temp1) * (2f / 3f - hue) * 6f;

            return temp1;
        }
    }
}
