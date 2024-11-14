using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class SheetManagerWindow : ResizableToolWindow
    {
        /// <summary>
        /// The tool window view for managing all sheets in a project.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public SheetManagerWindow(SheetManagerWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private readonly SheetManagerWindowViewModel _viewModel;

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
                ApplicationViewModel.Instance.DialogFactory.Show("Cannot delete active sheet", "This sheet cannot be deleted because it is open, switch to another sheet and then try again.");
                return;
            }

            ApplicationViewModel.Instance.DialogFactory.ShowYesNo("Delete sheet?", $"Are you sure you want to delete the sheet {sheetModel.Name}?", () =>
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
            _viewModel.MoveSheetDownInOrder(sheetModel);
        }

        private void OrderSheetUpButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel)) return;
            _viewModel.MoveSheetUpInOrder(sheetModel);
        }
    }
}
