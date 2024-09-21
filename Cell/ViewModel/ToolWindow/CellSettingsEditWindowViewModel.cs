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

        public override void ShowToolWindow()
        {
            _cellsToEdit.CollectionChanged += CellsToEditCollectionChanged;
            PickDisplayedCell();
        }

        public override void CloseToolWindow()
        {
            _cellsToEdit.CollectionChanged -= CellsToEditCollectionChanged;
            CellToDisplay = CellModel.Null;
        }

        public IEnumerable<CellModel> CellsBeingEdited => _cellsToEdit;

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
