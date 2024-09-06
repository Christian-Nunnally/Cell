using Cell.Common;
using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Media;

namespace Cell.ViewModel.Cells
{
    public class SheetViewModel : PropertyChangedBase
    {
        public static readonly SheetViewModel NullSheet = new(SheetModel.Null, null!, null!, null!, null!);
        private readonly UserCollectionLoader _userCollectionLoader;
        private readonly SheetTracker _sheetTracker;
        private readonly CellTracker _cellTracker;
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly SheetModel _model;
        private bool _enableMultiEditSelectedCells = true;
        private CellModel? oldSelectedCellState;
        private CellViewModel? selectedCellViewModel;

        public CellPopulateManager CellPopulateManager => _cellPopulateManager;

        public SheetViewModel(SheetModel model, CellPopulateManager cellPopulateManager, CellTracker cellTracker, SheetTracker sheetTracker, UserCollectionLoader userCollectionLoader)
        {
            _userCollectionLoader = userCollectionLoader;
            _sheetTracker = sheetTracker;
            _cellTracker = cellTracker;
            _cellPopulateManager = cellPopulateManager;
            _model = model;
            _model.Cells.CollectionChanged += CellsCollectionChanged;
            foreach (var cell in _model.Cells)
            {
                AddCellViewModel(cell);
            }
        }

        public ObservableCollection<CellViewModel> CellViewModels { get; set; } = [];

        public List<CellViewModel> HighlightedCellViewModels { get; } = [];

        public CellViewModel? SelectedCellViewModel
        {
            get => selectedCellViewModel;
            set
            {
                _enableMultiEditSelectedCells = false;
                if (selectedCellViewModel is not null)
                {
                    selectedCellViewModel.PropertyChanged -= PropertyChangedOnSelectedCell;
                    selectedCellViewModel.SelectionColor = new SolidColorBrush(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(selectedCellViewModel.BackgroundColorHex), 100));
                    selectedCellViewModel.SelectionBorderColor = new SolidColorBrush(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(selectedCellViewModel.BackgroundColorHex), 175));
                }
                selectedCellViewModel = value;
                oldSelectedCellState = selectedCellViewModel?.Model.Copy();
                if (selectedCellViewModel is not null)
                {
                    selectedCellViewModel.PropertyChanged += PropertyChangedOnSelectedCell;
                    selectedCellViewModel.SelectionColor = new SolidColorBrush(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(selectedCellViewModel.BackgroundColorHex), 100));
                    selectedCellViewModel.SelectionBorderColor = new SolidColorBrush(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(selectedCellViewModel.BackgroundColorHex), 175));
                }
                NotifyPropertyChanged(nameof(SelectedCellViewModel));
                _enableMultiEditSelectedCells = true;
            }
        }

        public List<CellViewModel> SelectedCellViewModels { get; } = [];

        public string SheetName => _model.Name;

        public CellTracker CellTracker => _cellTracker;

        public SheetTracker SheetTracker => _sheetTracker;

        public UserCollectionLoader UserCollectionLoader => _userCollectionLoader;

        public void HighlightCell(CellViewModel cellToHighlight, string color)
        {
            _enableMultiEditSelectedCells = false;
            cellToHighlight.HighlightCell(color);
            HighlightedCellViewModels.Add(cellToHighlight);
            _enableMultiEditSelectedCells = true;
        }

        public void MoveSelection(int columnOffset, int rowOffset)
        {
            if (SelectedCellViewModel is null) return;
            if (columnOffset > 0) columnOffset += SelectedCellViewModel.Model.CellsMergedToRight();
            if (rowOffset > 0) rowOffset += SelectedCellViewModel.Model.CellsMergedBelow();
            var cellToSelect = CellViewModels.FirstOrDefault(x => x.Column == SelectedCellViewModel.Column + columnOffset && x.Row == SelectedCellViewModel.Row + rowOffset);
            if (cellToSelect is null) return;
            UnselectAllCells();
            SelectCell(cellToSelect);
        }

        public void MoveSelectionDown() => MoveSelection(0, 1);

        public void MoveSelectionLeft() => MoveSelection(-1, 0);

        public void MoveSelectionRight() => MoveSelection(1, 0);

        public void MoveSelectionUp() => MoveSelection(0, -1);

        public void ReinstantiateCellsViewModel(CellViewModel? cellViewModel)
        {
            if (cellViewModel is null) return;
            SelectedCellViewModels.Remove(cellViewModel);
            HighlightedCellViewModels.Remove(cellViewModel);
            CellViewModels.Remove(cellViewModel);
            var newViewModel = CellViewModelFactory.Create(cellViewModel.Model, this);
            CellViewModels.Add(newViewModel);
        }

        public void SelectCell(CellModel cell) => SelectCell(CellViewModels.First(x => x.Model == cell));

        public void SelectCell(CellViewModel cell)
        {
            UnhighlightAllCells();
            if (!string.IsNullOrWhiteSpace(cell.Model.MergedWith))
            {
                cell = CellViewModels.First(x => x.Model.ID == cell.Model.MergedWith);
            }
            SelectedCellViewModel = cell;
            if (cell.IsSelected) return;
            cell.IsSelected = true;
            SelectedCellViewModels.Add(cell);
        }

        public void UnhighlightAllCells()
        {
            foreach (var cellToUnHighlight in HighlightedCellViewModels) cellToUnHighlight.UnhighlightCell();
            HighlightedCellViewModels.Clear();
        }

        public void UnmergeCell(CellModel mergedCell)
        {
            var cells = CellViewModels.Select(x => x.Model).Where(x => x.IsMergedWith(mergedCell));
            foreach (var cell in cells)
            {
                if (mergedCell == cell) continue;
                cell.MergedWith = string.Empty;
            }
            mergedCell.MergedWith = string.Empty;
        }

        public void UnselectAllCells()
        {
            foreach (var cell in SelectedCellViewModels.ToList()) UnselectCell(cell);
        }

        public void UnselectCell(CellViewModel cell)
        {
            SelectedCellViewModels.Remove(cell);
            cell.IsSelected = false;
            if (SelectedCellViewModel == cell) SelectedCellViewModel = null;
        }

        public void UpdateLayout()
        {
            _enableMultiEditSelectedCells = false;
            var layout = new CellLayout(CellViewModels, _cellTracker);
            layout.UpdateLayout();
            _enableMultiEditSelectedCells = true;
        }

        private void AddCellViewModel(CellModel newModel)
        {
            var newViewModel = CellViewModelFactory.Create(newModel, this);
            CellViewModels.Add(newViewModel);
            newViewModel.Model.PropertyChanged += CheckForCellTypePropertyChanged;
            UpdateLayout();
        }

        private void CheckForCellTypePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(CellModel.CellType)) return;
            var model = (CellModel)sender!;
            var viewModel = CellViewModels.FirstOrDefault(x => x.Model == model);
            if (viewModel == null) return;
            ReinstantiateCellsViewModel(viewModel);
        }

        private void CellsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CellModel cell in e.NewItems!) AddCellViewModel(cell);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (CellModel cell in e.OldItems!)RemoveCellViewModel(cell);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                CellViewModels.Clear();
                foreach (var cell in _model.Cells) AddCellViewModel(cell);
            }
        }

        private void RemoveCellViewModel(CellModel cell)
        {
            if (cell.IsMergedParent()) CellFormatEditWindow.UnmergeCell(cell, _cellTracker);
            cell.PropertyChanged -= CheckForCellTypePropertyChanged;
            var viewModel = CellViewModels.First(x => x.Model == cell);
            CellViewModels.Remove(viewModel);
            SelectedCellViewModels.Remove(viewModel);
        }

        private void PropertyChangedOnSelectedCell(object? sender, PropertyChangedEventArgs e)
        {
            if (!_enableMultiEditSelectedCells) return;
            if (sender is null) return;
            if (string.IsNullOrEmpty(e.PropertyName)) return;
            if (oldSelectedCellState == null) return;
            if (e.PropertyName == nameof(CellViewModel.IsSelected)) return;
            if (e.PropertyName == nameof(CellViewModel.IsHighlighted)) return;
            if (e.PropertyName == nameof(CellViewModel.ShouldShowSelectionBorder)) return;
            if (e.PropertyName == nameof(CellViewModel.ShouldShowSelectionFill)) return;
            if (e.PropertyName == nameof(CellViewModel.SelectionColor)) return;
            if (e.PropertyName == nameof(CellViewModel.SelectionBorderColor)) return;

            ApplicationViewModel.GetUndoRedoManager()?.StartRecordingUndoState();
            ApplicationViewModel.GetUndoRedoManager()?.RecordStateIfRecording(oldSelectedCellState);
            foreach (var cell in CellViewModels.Where(x => x.IsSelected).ToList())
            {
                if (cell == sender) continue;
                var cellType = cell.GetType();
                var selectedType = sender.GetType();
                PropertyInfo? cellProperty = cellType.GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo? selectedProperty = selectedType.GetProperty(e.PropertyName, BindingFlags.Public | BindingFlags.Instance);
                if (null != cellProperty && cellProperty.CanWrite && selectedProperty != null && selectedProperty.CanRead)
                {
                    cellProperty.SetValue(cell, selectedProperty.GetValue(sender), null);
                }
            }
            ApplicationViewModel.GetUndoRedoManager()?.FinishRecordingUndoState();
        }
    }
}
