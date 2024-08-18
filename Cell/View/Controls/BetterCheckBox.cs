using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cell.View.Controls
{
    internal class BetterCheckBox : Control
    {
        public static readonly DependencyProperty BackgroundWhenMouseOverProperty =
            DependencyProperty.Register("BackgroundWhenMouseOver", typeof(Brush), typeof(BetterCheckBox), new PropertyMetadata(default(Brush)));
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(BetterCheckBox), new PropertyMetadata(default(ICommand)));
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(BetterCheckBox), new PropertyMetadata(default(bool)));
        private Border? _checkboxBorder;
        public Brush BackgroundWhenMouseOver
        {
            get { return (Brush)GetValue(BackgroundWhenMouseOverProperty); }
            set { SetValue(BackgroundWhenMouseOverProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            SetCheckBoxBorder(GetTemplateChild("CheckBoxBorder") as Border ?? throw new Exception("Expected element named CheckBoxBorder in control template."));
        }

        private void CheckBoxBorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            Command?.Execute(null);
            IsChecked = !IsChecked;
        }

        private void SetCheckBoxBorder(Border value)
        {
            if (_checkboxBorder != null)
            {
                _checkboxBorder.MouseDown -= new MouseButtonEventHandler(CheckBoxBorderMouseDown);
            }
            _checkboxBorder = value;

            if (_checkboxBorder != null)
            {
                _checkboxBorder.MouseDown += new MouseButtonEventHandler(CheckBoxBorderMouseDown);
            }
        }
    }
}
