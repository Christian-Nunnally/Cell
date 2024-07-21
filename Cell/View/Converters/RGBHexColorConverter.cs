using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Cell.View.Converters
{
    public class RGBHexColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string hex) return Colors.Green;
            return ConvertHexStringToColor(hex);
        }

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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Color color ? $"#{color.R:X2}{color.G:X2}{color.B:X2}" : "#00ff00";
        }
    }
}
