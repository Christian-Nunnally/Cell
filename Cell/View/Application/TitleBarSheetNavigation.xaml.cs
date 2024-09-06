using Cell.Model;
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
        public TitleBarSheetNavigation()
        {
            InitializeComponent();
        }

        private TitleBarSheetNavigationViewModel? ViewModel => DataContext as TitleBarSheetNavigationViewModel;

        private void AddNewSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            if (ViewModel.IsAddingSheet)
            {
                if (!string.IsNullOrEmpty(ViewModel.NewSheetName))
                {
                    AddDefaultCells(ViewModel.NewSheetName);
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

        private void AddDefaultCells(string sheetName)
        {
            var corner = CellModelFactory.Create(0, 0, CellType.Corner, sheetName);
            ApplicationViewModel.Instance.CellTracker.AddCell(corner);
            var row = CellModelFactory.Create(1, 0, CellType.Row, sheetName);
            ApplicationViewModel.Instance.CellTracker.AddCell(row);
            var column = CellModelFactory.Create(0, 1, CellType.Column, sheetName);
            ApplicationViewModel.Instance.CellTracker.AddCell(column);
            var cell = CellModelFactory.Create(1, 1, CellType.Label, sheetName);
            ApplicationViewModel.Instance.CellTracker.AddCell(cell);
        }
    }
}
