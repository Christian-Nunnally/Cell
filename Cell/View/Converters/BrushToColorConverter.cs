using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Cell.View.Converters
{
    [ValueConversion(typeof(Brush), typeof(Color))]
    public class BrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                return brush.Color;
            }
            return Colors.Transparent; // Default value if conversion fails
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
