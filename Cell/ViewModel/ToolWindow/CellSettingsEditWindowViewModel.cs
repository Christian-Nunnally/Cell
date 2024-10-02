using Cell.Model;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// Tool window view model for editing cell settings.
    /// </summary>
    public class CellSettingsEditWindowViewModel : ToolWindowViewModel
    {
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private CellModel _cellToDisplay = CellModel.Null;

        /// <summary>
        /// Creates a new instance of <see cref="CellSettingsEditWindowViewModel"/>.
        /// </summary>
        /// <param name="cellsToEdit">The list of cells this edit window should act on. Can change at any time.</param>
        public CellSettingsEditWindowViewModel(ObservableCollection<CellModel> cellsToEdit)
        {
            _cellsToEdit = cellsToEdit;
        }

        /// <summary>
        /// Gets the cell that is currently being displayed in this tool window.
        /// </summary>
        public CellModel CellToDisplay
        {
            get => _cellToDisplay;
            set
            {
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged -= CellToDisplayPropertyChanged;
                _cellToDisplay = value;
                NotifyPropertyChanged(nameof(CellToDisplay));
                NotifyPropertyChanged(nameof(ToolWindowTitle));
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged += CellToDisplayPropertyChanged;
            }
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 200;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 200;

        /// <summary>
        /// Gets the minimum height this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumHeight => 100;

        /// <summary>
        /// Gets the minimum width this tool window is allowed to be reized to.
        /// </summary>
        public override double MinimumWidth => 100;

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle
        {
            get
            {
                var currentlySelectedCell = _cellsToEdit.FirstOrDefault();
                if (currentlySelectedCell is null) return "Select a cell to edit";
                return $"Cell settings editor - {currentlySelectedCell.GetName()}";
            }
        }

        /// <summary>
        /// Opens the code editor for the trigger function of the currently selected cell.
        /// </summary>
        public void EditTriggerFunction()
        {
            if (CellToDisplay == null) return;
            if (string.IsNullOrEmpty(CellToDisplay.TriggerFunctionName)) CellToDisplay.TriggerFunctionName = "Untitled";
            var function = ApplicationViewModel.Instance.PluginFunctionLoader.GetOrCreateFunction("void", CellToDisplay.TriggerFunctionName);
            var collectionNameToDataTypeMap = ApplicationViewModel.Instance.UserCollectionLoader.GenerateDataTypeForCollectionMap();
            var codeEditWindowViewModel = new CodeEditorWindowViewModel(function, CellToDisplay, collectionNameToDataTypeMap);
            ApplicationViewModel.Instance.ShowToolWindow(codeEditWindowViewModel, true);
        }

        /// <summary>
        /// Occurs when the tool window is really being closed.
        /// </summary>
        public override void HandleBeingClosed()
        {
            _cellsToEdit.CollectionChanged -= CellsToEditCollectionChanged;
            CellToDisplay = CellModel.Null;
        }

        /// <summary>
        /// Occurs when the tool window is being shown.
        /// </summary>
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
