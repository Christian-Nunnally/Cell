using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class SheetManagerWindow : UserControl, IResizableToolWindow
    {
        private readonly SheetManagerWindowViewModel _viewModel;
        private double _height = 400;
        private double _width = 400;
        public SheetManagerWindow(SheetManagerWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            _viewModel.UserSetWidth = GetWidth();
            _viewModel.UserSetHeight = GetHeight();
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetHeight() => _height;

        public string GetTitle() => "Sheet Manager";

        public List<CommandViewModel> GetToolBarCommands()
        {
            return
            [
                new CommandViewModel("Export", new RelayCommand(x => OpenExportWindow())),
                new CommandViewModel("Import", new RelayCommand(x => OpenImportWindow()))
            ];
        }

        public double GetWidth() => _width;

        public bool HandleBeingClosed()
        {
            return true;
        }

        public void SetHeight(double height)
        {
            _height = height;
            _viewModel.UserSetHeight = height;
        }

        public void SetWidth(double width)
        {
            _width = width;
            _viewModel.UserSetWidth = width;
        }

        private static void MakeSureSheetOrderingIsConsecutive()
        {
            var i = 0;
            foreach (var sheet in ApplicationViewModel.Instance.SheetTracker.OrderedSheets.ToList())
            {
                sheet.Order = i;
                i += 2;
            }
        }

        private void AddNewSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            var createSheetWindowViewModel = new CreateSheetWindowViewModel(ApplicationViewModel.Instance.SheetTracker);
            var createSheetWindow = new CreateSheetWindow(createSheetWindowViewModel);
            ApplicationViewModel.Instance.ShowToolWindow(createSheetWindow);
        }

        private void CopySheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel))
            {
                var sheetName = sheetModel.Name;
                CopySheet(sheetName);
            }
        }

        public void CopySheet(string sheetName)
        {
            var copiedSheetName = sheetName + "Copy";
            while (ApplicationViewModel.Instance.SheetTracker.Sheets.Any(x => x.Name == copiedSheetName)) copiedSheetName += "Copy";

            var copiedCells = ApplicationViewModel.Instance.SheetTracker.CreateUntrackedCopiesOfCellsInSheet(sheetName);
            ApplicationViewModel.Instance.CellLoader.UpdateIdentitiesOfCellsForNewSheet(copiedSheetName, copiedCells);
            ApplicationViewModel.Instance.SheetTracker.AddAndSaveCells(copiedCells);
        }

        private void DeleteSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel))
            {
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
        }

        private void GoToSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel))
            {
                ApplicationViewModel.Instance.GoToSheet(sheetModel.Name);
            }
        }

        private void OpenExportWindow()
        {
            var exportWindow = new ExportWindow(new ExportWindowViewModel());
            ApplicationViewModel.Instance.ShowToolWindow(exportWindow);
        }

        private void OpenImportWindow()
        {
            var importWindow = new ImportWindow(new ImportWindowViewModel());
            ApplicationViewModel.Instance.ShowToolWindow(importWindow);
        }

        private void OrderSheetDownButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            MakeSureSheetOrderingIsConsecutive();
            if (ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel))
            {
                sheetModel.Order += 3;
                _viewModel.RefreshSheetsList();
            }
            MakeSureSheetOrderingIsConsecutive();
        }

        private void OrderSheetUpButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            MakeSureSheetOrderingIsConsecutive();
            if (ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel))
            {
                sheetModel.Order -= 3;
                _viewModel.RefreshSheetsList();
            }
            MakeSureSheetOrderingIsConsecutive();
        }
    }
}
