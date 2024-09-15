using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Cell.ViewModel.ToolWindow
{
    public class CellSettingsEditWindowViewModel : PropertyChangedBase
    {
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private readonly CellTracker _cellTracker;
        private CellModel _cellToDisplay = CellModel.Null;
        private readonly PluginFunctionLoader _pluginFunctionLoader;

        private CellModel CellToDisplay
        {
            get => _cellToDisplay;
            set
            {
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged -= CellToDisplayPropertyChanged;
                _cellToDisplay = value;
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged += CellToDisplayPropertyChanged;
            }
        }

        private void CellToDisplayPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
        }

        public IEnumerable<CellModel> CellsBeingEdited => _cellsToEdit;

        public CellSettingsEditWindowViewModel(ObservableCollection<CellModel> cellsToEdit, CellTracker cellTracker, PluginFunctionLoader pluginFunctionLoader)
        {
            _pluginFunctionLoader = pluginFunctionLoader;
            _cellsToEdit = cellsToEdit;
            _cellTracker = cellTracker;
            _cellsToEdit.CollectionChanged += CellsToEditCollectionChanged;
            PickDisplayedCell();
        }

        private void PickDisplayedCell()
        {
            CellToDisplay = _cellsToEdit.Count > 0 ? _cellsToEdit[0] : CellModel.Null;

        }

        private void CellsToEditCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => PickDisplayedCell();
    }
}
