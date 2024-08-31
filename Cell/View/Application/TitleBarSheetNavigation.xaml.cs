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
        private readonly TitleBarSheetNavigationViewModel _viewModel = new();

        public TitleBarSheetNavigation()
        {
            DataContext = _viewModel;
            InitializeComponent();
        }

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsAddingSheet)
            {
                if (!string.IsNullOrEmpty(_viewModel.NewSheetName))
                {
                    ApplicationViewModel.Instance.GoToSheet(_viewModel.NewSheetName);
                }
                _viewModel.NewSheetName = string.Empty;
                _viewModel.IsAddingSheet = false;
            }
            else
            {
                _viewModel.IsAddingSheet = true;
                _viewModel.NewSheetName = "Untitled";
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
