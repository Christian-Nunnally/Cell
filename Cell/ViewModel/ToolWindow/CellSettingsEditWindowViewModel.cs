using Cell.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Cell.ViewModel.ToolWindow
{
    public class CellSettingsEditWindowViewModel : ToolWindowViewModel
    {
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private CellModel _cellToDisplay = CellModel.Null;
        public CellSettingsEditWindowViewModel(ObservableCollection<CellModel> cellsToEdit)
        {
            _cellsToEdit = cellsToEdit;
        }

        public IEnumerable<CellModel> CellsBeingEdited => _cellsToEdit;

        public override double DefaultHeight => 200;

        public override double DefaultWidth => 200;

        public override double MinimumHeight => 100;

        public override double MinimumWidth => 100;

        public CellModel CellToDisplay
        {
            get => _cellToDisplay;
            set
            {
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged -= CellToDisplayPropertyChanged;
                _cellToDisplay = value;
                NotifyPropertyChanged(nameof(CellToDisplay));
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged += CellToDisplayPropertyChanged;
            }
        }

        public override string ToolWindowTitle
        {
            get
            {
                var currentlySelectedCell = CellsBeingEdited.FirstOrDefault();
                if (currentlySelectedCell is null) return "Select a cell to edit";
                return $"Cell settings editor - {currentlySelectedCell.GetName()}";
            }
        }

        public override void HandleBeingClosed()
        {
            _cellsToEdit.CollectionChanged -= CellsToEditCollectionChanged;
            CellToDisplay = CellModel.Null;
        }

        public override void HandleBeingShown()
        {
            _cellsToEdit.CollectionChanged += CellsToEditCollectionChanged;
            PickDisplayedCell();
        }

        private void CellsToEditCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => PickDisplayedCell();

        private void CellToDisplayPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        private void PickDisplayedCell()
        {
            CellToDisplay = _cellsToEdit.Count > 0 ? _cellsToEdit[0] : CellModel.Null;
        }
    }
}
