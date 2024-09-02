using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;
using Cell.Persistence;
using Cell.Data;
using Cell.Common;

namespace Cell.View.ToolWindow
{
    public partial class SheetManagerWindow : UserControl, IResizableToolWindow
    {
        private double _width = 400;
        private double _height = 400;
        private readonly SheetManagerWindowViewModel _viewModel;
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

        private void OpenExportWindow()
        {
            var exportWindow = new ExportWindow(new ExportWindowViewModel());
            ApplicationViewModel.Instance.ApplicationView.ShowToolWindow(exportWindow);
        }

        private void OpenImportWindow()
        {
            var importWindow = new ImportWindow(new ImportWindowViewModel());
            ApplicationViewModel.Instance.ApplicationView.ShowToolWindow(importWindow);
        }

        private void DeleteSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel))
            {
                DialogWindow.ShowYesNoConfirmationDialog("Delete sheet?", $"Are you sure you want to delete the sheet {sheetModel.Name}?", () =>
                {
                    ApplicationViewModel.Instance.CellTracker.GetCellModelsForSheet(sheetModel.Name).ForEach(x => ApplicationViewModel.Instance.CellTracker.RemoveCell(x));    
                });
            }
        }

        private void CopySheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel))
            {
                PersistenceManager.CopySheet(sheetModel.Name);
            }
        }

        private void GoToSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<SheetModel>(sender, out var sheetModel))
            {
                ApplicationViewModel.Instance.GoToSheet(sheetModel.Name);
            }
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

        private static void MakeSureSheetOrderingIsConsecutive()
        {
            var i = 0;
            foreach (var sheet in SheetTracker.Instance.OrderedSheets.ToList())
            {
                sheet.Order = i;
                i += 2;
            }
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

        private void AddNewSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            var newSheetName = "NewSheet";
            var newSheetNameNumber = 1;
            while (SheetTracker.Instance.Sheets.Any(x => x.Name == $"{newSheetName}{newSheetNameNumber}"))
            {
                newSheetNameNumber += 1;
            }
            ApplicationViewModel.Instance.GoToSheet($"{newSheetName}{newSheetNameNumber}");
        }
    }
}
