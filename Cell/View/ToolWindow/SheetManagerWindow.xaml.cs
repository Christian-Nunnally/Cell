using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class SheetManagerWindow : ResizableToolWindow
    {
        public SheetManagerWindow(SheetManagerWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private SheetManagerWindowViewModel _viewModel;

        private static void MakeSureSheetOrderingIsConsecutive()
        {
            var i = 0;
            foreach (var sheet in ApplicationViewModel.Instance.SheetTracker.OrderedSheets.ToList())
            {
                sheet.Order = i;
                i += 2;
            }
        }

        private void CopySheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel)) return;
            var sheetName = sheetModel.Name;
            _viewModel.CopySheet(sheetName);
        }

        private void DeleteSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel)) return;
            if (ApplicationViewModel.Instance.SheetViewModel?.SheetName == sheetModel.Name)
            {
                DialogFactory.ShowDialog("Cannot delete active sheet", "This sheet cannot be deleted because it is open, switch to another sheet and then try again.");
                return;
            }

            DialogFactory.ShowYesNoConfirmationDialog("Delete sheet?", $"Are you sure you want to delete the sheet {sheetModel.Name}?", () =>
            {
                ApplicationViewModel.Instance.CellTracker.GetCellModelsForSheet(sheetModel.Name).ForEach(x => ApplicationViewModel.Instance.CellTracker.RemoveCell(x));
            });
        }

        private void GoToSheetButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel)) return;
            ApplicationViewModel.Instance.GoToSheet(sheetModel.Name);
        }

        private void OrderSheetDownButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel)) return;
            MakeSureSheetOrderingIsConsecutive();
            sheetModel.Order += 3;
            _viewModel.RefreshSheetsList();
            MakeSureSheetOrderingIsConsecutive();
        }

        private void OrderSheetUpButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel)) return;
            MakeSureSheetOrderingIsConsecutive();
            sheetModel.Order -= 3;
            _viewModel.RefreshSheetsList();
            MakeSureSheetOrderingIsConsecutive();
        }
    }
}
