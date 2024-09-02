using Cell.ViewModel.Application;
using System.Windows;
using System.Windows.Controls;

namespace Cell.View.Application
{
    /// <summary>
    /// Interaction logic for TitleBarSheetNavigation.xaml
    /// </summary>
    public partial class TitleBarSheetNavigation : UserControl
    {
        private TitleBarSheetNavigationViewModel? ViewModel => DataContext as TitleBarSheetNavigationViewModel;

        public TitleBarSheetNavigation()
        {
            InitializeComponent();
        }

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            if (ViewModel.IsAddingSheet)
            {
                if (!string.IsNullOrEmpty(ViewModel.NewSheetName))
                {
                    ApplicationViewModel.Instance.GoToSheet(ViewModel.NewSheetName);
                }
                ViewModel.NewSheetName = string.Empty;
                ViewModel.IsAddingSheet = false;
            }
            else
            {
                ViewModel.IsAddingSheet = true;
                ViewModel.NewSheetName = "Untitled";
            }
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
