using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.Application
{
    /// <summary>
    /// Interaction logic for TitleBarSheetNavigation.xaml
    /// </summary>
    public partial class TitleBarSheetNavigation : UserControl
    {
        public TitleBarSheetNavigation()
        {
            InitializeComponent();
        }

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            var createSheetWindowViewModel = new CreateSheetWindowViewModel(ApplicationViewModel.Instance.SheetTracker);
            var createSheetWindow = new CreateSheetWindow(createSheetWindowViewModel);
            ApplicationViewModel.Instance.ShowToolWindow(createSheetWindow);
        }

        private void GoToSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.Content is not Label label) return;
            if (label.Content is not string sheetName) return;
            ApplicationViewModel.Instance.GoToSheet(sheetName);
        }
    }
}
