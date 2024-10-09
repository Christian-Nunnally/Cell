using Cell.Common;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Cell.View.Converters
{
    /// <summary>
    /// Converts a hex color string to a color.
    /// </summary>
    public class HexToColorConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string hex) return Colors.Green;
            return ColorAdjuster.ConvertHexStringToColor(hex);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Color color ? ColorAdjuster.ConvertColorToHexString(color) : "#00ff00";
        }
    }
}
