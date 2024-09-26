using Cell.Data;
using Cell.Model;
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

        public override double MinimumHeight => 100;

        public override double MinimumWidth => 200;

        public override double DefaultHeight => 300;

        public override double DefaultWidth => 400;

        private void SheetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshSheetsList();

        public string SelectedSheetName { get; set; } = string.Empty;

        public IEnumerable<SheetModel> FilteredSheets => _sheetTracker.OrderedSheets.Where(x => x.Name.Contains(SheetsListBoxFilterText)).OrderBy(x => x.Order);

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
