using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cell.View.Controls
{
    /// <summary>
    /// A checkbox control that allows for more custom styling, like the mouse over background color.
    /// </summary>
    public class BetterCheckBox : Control
    {
        /// <summary>
        /// The dependency property for the BackgroundWhenMouseOver property.
        /// </summary>
        public static readonly DependencyProperty BackgroundWhenMouseOverProperty =
            DependencyProperty.Register("BackgroundWhenMouseOver", typeof(Brush), typeof(BetterCheckBox), new PropertyMetadata(default(Brush)));

        /// <summary>
        /// The dependency property for the Command property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(BetterCheckBox), new PropertyMetadata(default(ICommand)));

        /// <summary>
        /// The dependency property for the IsChecked property.
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(BetterCheckBox), new PropertyMetadata(default(bool)));
        
        private Border? _checkboxBorder;
        
        /// <summary>
        /// Gets or sets the background brush when the mouse is over the checkbox.
        /// </summary>
        public Brush BackgroundWhenMouseOver
        {
            get { return (Brush)GetValue(BackgroundWhenMouseOverProperty); }
            set { SetValue(BackgroundWhenMouseOverProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the checkbox is checked.
        /// </summary>
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        /// <summary>
        /// Overrides the OnApplyTemplate method to set the checkbox border.
        /// </summary>
        /// <exception cref="Exception">Control template is invalid.</exception>
        public override void OnApplyTemplate()
        {
            SetCheckBoxBorder(GetTemplateChild("CheckBoxBorder") as Border ?? throw new Exception("Expected element named CheckBoxBorder in control template."));
        }

        private void CheckBoxBorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
        }

        private void SetCheckBoxBorder(Border value)
        {
            if (_checkboxBorder != null)
            {
                _checkboxBorder.MouseDown -= new MouseButtonEventHandler(CheckBoxBorderMouseDown);
                _checkboxBorder.PreviewKeyDown -= new KeyEventHandler(CheckBoxBorderKeyDown);
            }
            _checkboxBorder = value;

            if (_checkboxBorder != null)
            {
                _checkboxBorder.MouseDown += new MouseButtonEventHandler(CheckBoxBorderMouseDown);
                _checkboxBorder.PreviewKeyDown += new KeyEventHandler(CheckBoxBorderKeyDown);
            }
        }

        private void CheckBoxBorderKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                IsChecked = !IsChecked;
            }
        }
    }
}
