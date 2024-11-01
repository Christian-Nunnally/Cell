using Cell.Core.Common;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Cell.View.Converters
{
    /// <summary>
    /// Maps colors to other colors to quickly change theme of a component that uses user selected colors.
    /// </summary>
    public class ThemeColorConverter : IValueConverter
    {
        public static bool IsDarkMode { get; set; } = true;

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush originalColor)
            {
                return IsDarkMode ? new SolidColorBrush(ColorAdjuster.InvertBrightness(originalColor.Color)) : originalColor;
            }
            return value;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
