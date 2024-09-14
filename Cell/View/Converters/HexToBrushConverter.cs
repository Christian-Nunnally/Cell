using Cell.Common;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Cell.View.Converters
{
    [ValueConversion(typeof(Brush), typeof(Color))]
    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string hex) return Colors.Green;
            return ColorAdjuster.ConvertHexStringToBrush(hex);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is SolidColorBrush brush ? ColorAdjuster.ConvertColorToHexString(brush.Color) : "#00ff00";
        }
    }
}
