using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cell.View.Controls
{
    internal class BetterComboBox : ComboBox
    {
        public Brush HighlightBrush
        {
            get { return (Brush)GetValue(HighlightBrushProperty); }
            set { SetValue(HighlightBrushProperty, value); }
        }

        public static readonly DependencyProperty HighlightBrushProperty =
            DependencyProperty.Register("HighlightBrush", typeof(Brush), typeof(BetterComboBox), new PropertyMetadata(Brushes.LawnGreen));
    }
}
