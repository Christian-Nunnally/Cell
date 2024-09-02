using Cell.Common;
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
            return ColorAdjuster.ConvertHexStringToColor(hex);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Color color ? ColorAdjuster.ConvertColorToHexString(color) : "#00ff00";
        }
    }
}
