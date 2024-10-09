using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cell.View.Controls
{
    /// <summary>
    /// A combobox control that allows for more custom styling, like the highlight brush.
    /// </summary>
    public class BetterComboBox : ComboBox
    {
        /// <summary>
        /// The dependency property for the HighlightBrush property.
        /// </summary>
        public static readonly DependencyProperty HighlightBrushProperty =
            DependencyProperty.Register("HighlightBrush", typeof(Brush), typeof(BetterComboBox), new PropertyMetadata(Brushes.LawnGreen));

        /// <summary>
        /// Gets or sets the brush used to highlight the combobox when the mouse is over it.
        /// </summary>
        public Brush HighlightBrush
        {
            get { return (Brush)GetValue(HighlightBrushProperty); }
            set { SetValue(HighlightBrushProperty, value); }
        }
    }
}
