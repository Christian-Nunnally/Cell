using Cell.Execution;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// Tool window view model for editing the content of a cell. Allows setting the text of the cell, index of the cell and set the populate function.
    /// </summary>
    public class CellContentEditWindowViewModel : ToolWindowViewModel
    {
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private CellModel _cellToDisplay = CellModel.Null;
        private string _multiUseUserInputText = string.Empty;
        public CellContentEditWindowViewModel(ObservableCollection<CellModel> cellsToEdit, CellPopulateManager cellPopulateManager)
        {
            _cellPopulateManager = cellPopulateManager;
            _cellsToEdit = cellsToEdit;
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 90;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 500;

        public int Index
        {
            get => CellToDisplay.Index;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    CellToDisplay.Index = value;
                }
            }
        }

        /// <summary>
        /// Gets whether the edit function button should be visible in the UI.
        /// </summary>
        public bool IsEditFunctionButtonVisible => _multiUseUserInputText.StartsWith('=');

        /// <summary>
        /// Gets the minimum height this tool window is allowed to be resized to.
        /// </summary>
        public override double MinimumHeight => 80;

        /// <summary>
        /// Gets the minimum width this tool window is allowed to be resized to.
        /// </summary>
        public override double MinimumWidth => 200;

        public string MultiUseUserInputText
        {
            get => _multiUseUserInputText;
            set
            {
                _multiUseUserInputText = value;
                NotifyPropertyChanged(nameof(MultiUseUserInputText));
                NotifyPropertyChanged(nameof(IsEditFunctionButtonVisible));
            }
        }

        /// <summary>
        /// Provides a list of commands to display in the title bar of the tool window.
        /// </summary>
        public override List<CommandViewModel> ToolBarCommands =>
        [
            new CommandViewModel("Auto-Index", IndexSelectedCells) { ToolTip = "Sets the index of selected cells in an incrementing fashion (0, 1, 2...). Will work horizontially if only one row is selected." },
        ];

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle
        {
            get
            {
                var currentlySelectedCell = _cellsToEdit.FirstOrDefault();
                if (currentlySelectedCell is null) return "Select a cell to edit";
                return $"Value editor - {currentlySelectedCell.GetName()}";
            }
        }

        private CellModel CellToDisplay
        {
            get => _cellToDisplay;
            set
            {
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged -= CellToDisplayPropertyChanged;
                _cellToDisplay = value;
                if (_cellToDisplay != CellModel.Null) _cellToDisplay.PropertyChanged += CellToDisplayPropertyChanged;
                NotifyPropertyChanged(nameof(CellToDisplay));
                NotifyPropertyChanged(nameof(Index));
                NotifyPropertyChanged(nameof(ToolWindowTitle));
                if (!string.IsNullOrEmpty(_cellToDisplay.PopulateFunctionName)) MultiUseUserInputText = $"={_cellToDisplay.PopulateFunctionName}";
                else MultiUseUserInputText = _cellToDisplay.Text;
            }
        }

        /// <summary>
        /// Opens the code editor for the current cells populate function.
        /// </summary>
        public void EditPopulateFunction()
        {
            if (CellToDisplay == null) return;
            if (string.IsNullOrEmpty(CellToDisplay.PopulateFunctionName)) CellToDisplay.PopulateFunctionName = "Untitled";
            var function = ApplicationViewModel.Instance.PluginFunctionLoader.GetOrCreateFunction("object", CellToDisplay.PopulateFunctionName);
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

        public void SubmitMultiUseUserInputText()
        {
            if (_multiUseUserInputText.StartsWith('=')) CellToDisplay.PopulateFunctionName = _multiUseUserInputText[1..].Trim();
            else CellToDisplay.Text = _multiUseUserInputText;
        }

        private static void IndexSelectedCells()
        {
            if (ApplicationViewModel.Instance.SheetViewModel == null) return;
            var selectedCells = ApplicationViewModel.Instance.SheetViewModel.CellSelector.SelectedCells.ToList();
            var leftmost = selectedCells.Select(x => x.Column).Min();
            var topmost = selectedCells.Select(x => x.Row).Min();
            var topLeftCell = selectedCells.FirstOrDefault(x => x.Row == topmost && x.Column == leftmost);
            if (topLeftCell is null) return;
            var isLinearSelection = selectedCells.Select(x => x.Column).Distinct().Count() == 1 || selectedCells.Select(x => x.Row).Distinct().Count() == 1;
            foreach (var selectedCell in selectedCells)
            {
                if (selectedCell == topLeftCell) continue;
                var distance = isLinearSelection
                    ? (selectedCell.Column - topLeftCell.Column) + (selectedCell.Row - topLeftCell.Row)
                    : selectedCell.Row - topLeftCell.Row;
                selectedCell.Index = topLeftCell.Index + distance;
            }
        }

        private void CellsToEditCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => PickDisplayedCell();

        private void CellToDisplayPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.Text)) NotifyPropertyChanged(nameof(CellModel.Text));
        }

        private void PickDisplayedCell()
        {
            CellToDisplay = _cellsToEdit.Count > 0 ? _cellsToEdit[0] : CellModel.Null;
        }
    }
}
