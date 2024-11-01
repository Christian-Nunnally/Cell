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
        /// <summary>
        /// Gets or sets a value indicating whether the current theme is dark mode.
        /// </summary>
        public static bool IsDarkMode { get; set; } = true;

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush originalColor)
            {
                return IsDarkMode ? originalColor : new SolidColorBrush(ColorAdjuster.InvertBrightness(originalColor.Color));
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
