﻿using Cell.Core.Common;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution.CodeCompletion;
using Cell.Core.Execution.Functions;
using Cell.Model;
using Cell.View.Application;
using Cell.ViewModel.Application;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// Tool window view model for editing the content of a cell. Allows setting the text of the cell, index of the cell and set the populate function.
    /// </summary>
    public class CellContentEditWindowViewModel : ToolWindowViewModel
    {
        private readonly Logger _logger;
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private readonly FunctionTracker _functionTracker;
        private CellModel _cellToDisplay = CellModel.Null;
        private string _multiUseUserInputText = string.Empty;
        private bool arePendingChangesInMultiEditTextBox;

        /// <summary>
        /// Creates a new instance of the <see cref="CellContentEditWindowViewModel"/> class.
        /// </summary>
        /// <param name="cellsToEdit">The dynamic list of cells being edited by this tool window.</param>
        /// <param name="functionTracker">The function tracker to get populate and trigger functions from.</param>
        /// <param name="logger">The logger to log messeges to.</param>
        public CellContentEditWindowViewModel(ObservableCollection<CellModel> cellsToEdit, FunctionTracker functionTracker, Logger logger)
        {
            _logger = logger;
            _cellsToEdit = cellsToEdit;
            _functionTracker = functionTracker;
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 35;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 500;

        /// <summary>
        /// Gets or sets the index of the cells being edited, recording the state to the undo/redo manager is recording.
        /// </summary>
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
        /// Gets the minimum width this tool window is allowed to be resized to.
        /// </summary>
        public override double MinimumWidth => 350;

        /// <summary>
        /// Gets or sets the text that the user has input into the content editor text box.
        /// 
        /// This might be normal text or a function name. If it is a function name it should start with an equals sign.
        /// </summary>
        public string MultiUseUserInputText
        {
            get => _multiUseUserInputText;
            set
            {
               if (value == _multiUseUserInputText) return;
                _multiUseUserInputText = value;
                NotifyPropertyChanged(nameof(MultiUseUserInputText));
                NotifyPropertyChanged(nameof(IsEditFunctionButtonVisible));
            }
        }

        public bool ArePendingChangesInMultiEditTextBox
        {
            get => arePendingChangesInMultiEditTextBox;
            set 
            {
                if (arePendingChangesInMultiEditTextBox == value) return;
                arePendingChangesInMultiEditTextBox = value;
                NotifyPropertyChanged(nameof(ArePendingChangesInMultiEditTextBox));
            }
        }

        /// <summary>
        /// Gets the string displayed in top bar of this tool window.
        /// </summary>
        public override string ToolWindowTitle
        {
            get
            {
                var currentlySelectedCell = _cellsToEdit.FirstOrDefault();
                if (currentlySelectedCell is null) return "Select a cell to edit";
                return $"Value editor - {currentlySelectedCell.Location.UserFriendlyLocationString}";
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
                NotifyPropertyChanged(nameof(TriggerFunctionNameTextboxText));
                if (!string.IsNullOrEmpty(_cellToDisplay.PopulateFunctionName)) MultiUseUserInputText = $"={_cellToDisplay.PopulateFunctionName}";
                else MultiUseUserInputText = _cellToDisplay.Text;
            }
        }

        /// <summary>
        /// Opens the code editor for the current cells populate function.
        /// </summary>
        public void EditPopulateFunction()
        {
            if (CellToDisplay is null) return;
            if (string.IsNullOrEmpty(CellToDisplay.PopulateFunctionName))
            {
                ApplicationViewModel.Instance.DialogFactory?.Show("No function name to edit", "Set the name of the function before editing it.");
                return;
            }
            var function = _functionTracker.GetOrCreateFunction("object", CellToDisplay.PopulateFunctionName);
            EditFunction(function);
        }

        /// <summary>
        /// Gets or sets the name of the trigger function the editing cells should use and records to the undo stack.
        /// </summary>
        public string TriggerFunctionNameTextboxText
        {
            get => CellToDisplay.TriggerFunctionName;
            set
            {
                foreach (var cell in _cellsToEdit)
                {
                    ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                    cell.TriggerFunctionName = value;
                }
                NotifyPropertyChanged(nameof(TriggerFunctionNameTextboxText));
            }
        }

        /// <summary>
        /// Opens the code editor for the current cells populate function.
        /// </summary>
        public void EditTriggerFunction()
        {
            if (CellToDisplay is null) return;
            if (string.IsNullOrEmpty(CellToDisplay.TriggerFunctionName))
            {
                ApplicationViewModel.Instance.DialogFactory?.Show("No function name to edit", "Set the name of the function before editing it.");
                return;
            }
            var function = _functionTracker.GetOrCreateFunction("void", CellToDisplay.TriggerFunctionName);
            EditFunction(function);
        }

        private void EditFunction(CellFunction function)
        {
            if (ApplicationViewModel.Instance.CellTracker is null) return;
            var userCollectionTracker = ApplicationViewModel.Instance.UserCollectionTracker;
            if (userCollectionTracker is null) return;
            var collectionNameToPropertyNameMap = userCollectionTracker.GeneratePropertyNamesForCollectionMap();
            var testingContext = new TestingContext(ApplicationViewModel.Instance.CellTracker, userCollectionTracker, CellToDisplay, _functionTracker, _logger);
            var codeEditWindowViewModel = new CodeEditorWindowViewModel(function, CellToDisplay, collectionNameToPropertyNameMap, testingContext, _logger);

            if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) ApplicationViewModel.Instance.DockToolWindow(codeEditWindowViewModel, WindowDockType.DockedBottom, true);
            else ApplicationViewModel.Instance.ShowToolWindow(codeEditWindowViewModel, true);
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

        /// <summary>
        /// Causes the cells to update their text to the value in the <see cref="MultiUseUserInputText"/> property, or the populate function if the text starts with an equals sign.
        /// </summary>
        public void SubmitMultiUseUserInputText()
        {
            if (_multiUseUserInputText.StartsWith('=')) SetCellsPopulateFunctionFromMultiUseEditBox();
            else SetCellsTextFromMultiUseEditBox();
            ArePendingChangesInMultiEditTextBox = false;
        }

        private void SetCellsTextFromMultiUseEditBox()
        {
            foreach (var cell in _cellsToEdit)
            {
                cell.PopulateFunctionName = string.Empty;
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                cell.Text = _multiUseUserInputText;
            }
        }

        private void SetCellsPopulateFunctionFromMultiUseEditBox()
        {
            var functionName = _multiUseUserInputText[1..].Trim();
            foreach (var cell in _cellsToEdit)
            {
                ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(cell);
                cell.PopulateFunctionName = functionName;
            }
        }

        /// <summary>
        /// Automatically indexes the selected cells based on the top left cell, incrementing by one for each cell to the right and down.
        /// </summary>
        public void AutoIndexSelectedCells()
        {
            var selectedCells = _cellsToEdit.ToList();
            var leftmost = selectedCells.Select(x => x.Location.Column).Min();
            var topmost = selectedCells.Select(x => x.Location.Row).Min();
            var topLeftCell = selectedCells.FirstOrDefault(x => x.Location.Row == topmost && x.Location.Column == leftmost);
            if (topLeftCell is null) return;
            var isLinearSelection = selectedCells.Select(x => x.Location.Column).Distinct().Count() == 1 || selectedCells.Select(x => x.Location.Row).Distinct().Count() == 1;
            foreach (var selectedCell in selectedCells)
            {
                if (selectedCell == topLeftCell) continue;
                var distance = isLinearSelection
                    ? (selectedCell.Location.Column - topLeftCell.Location.Column) + (selectedCell.Location.Row - topLeftCell.Location.Row)
                    : selectedCell.Location.Row - topLeftCell.Location.Row;
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

        internal IEnumerable<ICompletionData> GetPopulateFunctionSuggestions()
        {
            var suggestions = new List<ICompletionData>();
            foreach (var function in _functionTracker.Functions)
            {
                if (function.Model.ReturnType == "void") continue;
                suggestions.Add(new CodeCompletionData(function.Model.Name, function.Model.Name, function.Model.Description, FontAwesome.Sharp.IconChar.Code));
            }
            return suggestions;
        }
    }
}
