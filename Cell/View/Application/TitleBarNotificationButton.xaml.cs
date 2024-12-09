using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.Application
{
    /// <summary>
    /// Interaction logic for TitleBarSheetNavigation.xaml
    /// </summary>
    public partial class TitleBarNotificationButton : UserControl
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TitleBarSheetNavigation"/>.
        /// </summary>
        public TitleBarNotificationButton()
        {
            InitializeComponent();
        }

        private void ShowNotifcationsWindowButtonClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not TitleBarNotificationButtonViewModel titleBarNotificationButtonViewModel) return;
            titleBarNotificationButtonViewModel.ShowNotifcationsWindow();
        }
    }
}
