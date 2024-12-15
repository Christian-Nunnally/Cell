using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Cell.View.Converters
{
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && string.IsNullOrWhiteSpace(text))
            {
                return Visibility.Collapsed; // Collapses the Label if the text is empty
            }

            return Visibility.Visible; // Otherwise, the Label is visible
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
