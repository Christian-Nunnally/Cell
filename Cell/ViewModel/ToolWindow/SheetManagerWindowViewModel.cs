using Cell.Data;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Collections.Specialized;

namespace Cell.ViewModel.ToolWindow
{
    public class SheetManagerWindowViewModel : ToolWindowViewModel
    {
        private string _sheetsListBoxFilterText = string.Empty;
        private readonly SheetTracker _sheetTracker;

        public SheetManagerWindowViewModel(SheetTracker sheetTracker)
        {
            _sheetTracker = sheetTracker;
        }

        /// <summary>
        /// Provides a list of commands to display in the title bar of the tool window.
        /// </summary>
        public override List<CommandViewModel> ToolBarCommands => 
        [
            new CommandViewModel("Export", OpenExportWindow) { ToolTip = "Opens the export tool window." },
            new CommandViewModel("Import", OpenImportWindow) { ToolTip = "Opens the import tool window" },
            new CommandViewModel("New Sheet", OpenAddNewSheetWindow) { ToolTip = "Opens the create new sheet tool window." }
        ];

        private void OpenExportWindow()
        {
            var exportWindow = new ExportWindowViewModel();
            ApplicationViewModel.Instance.ShowToolWindow(exportWindow);
        }

        private void OpenImportWindow()
        {
            var importWindow = new ImportWindowViewModel();
            ApplicationViewModel.Instance.ShowToolWindow(importWindow);
        }

        private void OpenAddNewSheetWindow()
        {
            var createSheetWindowViewModel = new CreateSheetWindowViewModel(ApplicationViewModel.Instance.SheetTracker);
            ApplicationViewModel.Instance.ShowToolWindow(createSheetWindowViewModel);
        }

        public override double MinimumHeight => 100;

        public override double MinimumWidth => 200;

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 300;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 400;

        private void SheetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshSheetsList();

        public string SelectedSheetName { get; set; } = string.Empty;

        public IEnumerable<SheetModel> FilteredSheets => _sheetTracker.OrderedSheets.Where(x => x.Name.Contains(SheetsListBoxFilterText)).OrderBy(x => x.Order);

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle => "Sheet Manager";

        public void RefreshSheetsList()
        {
            NotifyPropertyChanged(nameof(FilteredSheets));
        }

        public string SheetsListBoxFilterText
        {
            get => _sheetsListBoxFilterText; set
            {
                if (_sheetsListBoxFilterText == value) return;
                _sheetsListBoxFilterText = value;
                NotifyPropertyChanged(nameof(SheetsListBoxFilterText));
                RefreshSheetsList();
            }
        }

        public override void HandleBeingShown()
        {
            _sheetTracker.OrderedSheets.CollectionChanged += SheetsCollectionChanged;
        }

        public override void HandleBeingClosed()
        {
            _sheetTracker.OrderedSheets.CollectionChanged -= SheetsCollectionChanged;
        }

        public void CopySheet(string sheetName)
        {
            var copiedSheetName = sheetName + "Copy";
            while (_sheetTracker.Sheets.Any(x => x.Name == copiedSheetName)) copiedSheetName += "Copy";

            var copiedCells = _sheetTracker.CreateUntrackedCopiesOfCellsInSheet(sheetName);
            _sheetTracker.UpdateIdentitiesOfCellsForNewSheet(copiedSheetName, copiedCells);
            _sheetTracker.AddAndSaveCells(copiedCells);
        }
    }
}
