using System.Globalization;
using System.Windows.Data;

namespace Cell.View.Converters
{
    /// <summary>
    /// Converts an enum to a string.
    /// </summary>
    public class EnumToStringConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string EnumString;
            try
            {
                EnumString = Enum.GetName(value.GetType(), value) ?? string.Empty;
                return EnumString;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
