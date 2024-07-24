using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cell.View.Controls
{
    internal class BetterCheckBox : Control
    {
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(BetterCheckBox), new PropertyMetadata(default(bool)));

        public Brush BackgroundWhenMouseOver
        {
            get { return (Brush)GetValue(BackgroundWhenMouseOverProperty); }
            set { SetValue(BackgroundWhenMouseOverProperty, value); }
        }

        public static readonly DependencyProperty BackgroundWhenMouseOverProperty =
            DependencyProperty.Register("BackgroundWhenMouseOver", typeof(Brush), typeof(BetterCheckBox), new PropertyMetadata(default(Brush)));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(BetterCheckBox), new PropertyMetadata(default(ICommand)));

        private Border? _checkboxBorder;

        private Border CheckBoxBorder
        {
            get => _checkboxBorder;
            set
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

        public override void OnApplyTemplate()
        {
            CheckBoxBorder = GetTemplateChild("CheckBoxBorder") as Border ?? throw new Exception("Expected element named CheckBoxBorder in control template.");
        }

        private void CheckBoxBorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            Command?.Execute(null);
            IsChecked = !IsChecked;
        }
    }
}
