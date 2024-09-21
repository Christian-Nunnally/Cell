using Cell.Common;
using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Media;

namespace Cell.ViewModel.Cells
{
    public class SheetViewModel : PropertyChangedBase
    {
        public static readonly SheetViewModel NullSheet = new(SheetModel.Null, null!, null!, null!, null!, null!, null!, null!);
        private readonly ApplicationSettings _applicationSettings;
        private readonly Dictionary<CellModel, CellViewModel> _cellModelToCellViewModelMap = [];
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly CellTracker _cellTracker;
        private readonly SheetModel _model;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly SheetTracker _sheetTracker;
        private readonly UserCollectionLoader _userCollectionLoader;
        private double _sheetHeight;
        private double _sheetWidth;
        private CellViewModel? selectedCellViewModel;
        public SheetViewModel(
            SheetModel model,
            CellPopulateManager cellPopulateManager,
            CellTracker cellTracker,
            SheetTracker sheetTracker,
            CellSelector cellSelector,
            UserCollectionLoader userCollectionLoader,
            ApplicationSettings applicationSettings,
            PluginFunctionLoader pluginFunctionLoader)
        {
            _pluginFunctionLoader = pluginFunctionLoader;
            _applicationSettings = applicationSettings;
            _userCollectionLoader = userCollectionLoader;
            _cellTracker = cellTracker;
            _sheetTracker = sheetTracker;
            CellSelector = cellSelector;
            if (cellSelector != null) cellSelector.SelectedCells.CollectionChanged += SelectedCellsChanged;
            _cellPopulateManager = cellPopulateManager;
            _model = model;
            _model.Cells.CollectionChanged += CellsCollectionChanged;
            foreach (var cell in _model.Cells)
            {
                AddCellViewModel(cell);
            }
            UpdateLayout();
        }

        public ApplicationSettings ApplicationSettings => _applicationSettings;

        public CellPopulateManager CellPopulateManager => _cellPopulateManager;

        public CellSelector CellSelector { get; private set; }

        public CellTracker CellTracker => _cellTracker;

        public ObservableCollection<CellViewModel> CellViewModels { get; set; } = [];

        public List<CellViewModel> HighlightedCellViewModels { get; } = [];

        public PluginFunctionLoader PluginFunctionLoader => _pluginFunctionLoader;

        public CellViewModel? SelectedCellViewModel
        {
            get => selectedCellViewModel;
            set
            {
                if (selectedCellViewModel is not null)
                {
                    selectedCellViewModel.SelectionColor = new SolidColorBrush(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(selectedCellViewModel.BackgroundColorHex), 100));
                    selectedCellViewModel.SelectionBorderColor = new SolidColorBrush(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(selectedCellViewModel.BackgroundColorHex), 175));
                }
                selectedCellViewModel = value;
                if (selectedCellViewModel is not null)
                {
                    selectedCellViewModel.SelectionColor = new SolidColorBrush(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(selectedCellViewModel.BackgroundColorHex), 100));
                    selectedCellViewModel.SelectionBorderColor = new SolidColorBrush(ColorAdjuster.GetHighlightColor((Color)ColorConverter.ConvertFromString(selectedCellViewModel.BackgroundColorHex), 175));
                }
                NotifyPropertyChanged(nameof(SelectedCellViewModel));
            }
        }

        public double SheetHeight
        {
            get => _sheetHeight;
            private set
            {
                if (_sheetHeight == value) return;
                _sheetHeight = value;
                NotifyPropertyChanged(nameof(SheetHeight));
            }
        }

        public string SheetName => _model.Name;

        public SheetTracker SheetTracker => _sheetTracker;

        public double SheetWidth
        {
            get => _sheetWidth;
            private set
            {
                if (_sheetWidth == value) return;
                _sheetWidth = value;
                NotifyPropertyChanged(nameof(SheetWidth));
            }
        }

        public UserCollectionLoader UserCollectionLoader => _userCollectionLoader;

        public CellViewModel GetCellViewModel(CellModel cellModel) => _cellModelToCellViewModelMap[cellModel];

        public void HighlightCell(CellViewModel cellToHighlight, string color)
        {
            cellToHighlight.HighlightCell(color);
            HighlightedCellViewModels.Add(cellToHighlight);
        }

        public void MoveSelection(int columnOffset, int rowOffset)
        {
            if (SelectedCellViewModel is null) return;
            if (columnOffset > 0) columnOffset += SelectedCellViewModel.Model.CellsMergedToRight();
            if (rowOffset > 0) rowOffset += SelectedCellViewModel.Model.CellsMergedBelow();
            var cellToSelect = CellViewModels.FirstOrDefault(x => x.Column == SelectedCellViewModel.Column + columnOffset && x.Row == SelectedCellViewModel.Row + rowOffset);
            if (cellToSelect is null) return;
            CellSelector.UnselectAllCells();
            CellSelector.SelectCell(cellToSelect.Model);
        }

        public void MoveSelectionDown() => MoveSelection(0, 1);

        public void MoveSelectionLeft() => MoveSelection(-1, 0);

        public void MoveSelectionRight() => MoveSelection(1, 0);

        public void MoveSelectionUp() => MoveSelection(0, -1);

        public void ReinstantiateCellsViewModel(CellViewModel cellViewModel)
        {
            RemoveCellViewModel(cellViewModel.Model);
            AddCellViewModel(cellViewModel.Model);
        }

        public void UnhighlightAllCells()
        {
            foreach (var cellToUnHighlight in HighlightedCellViewModels) cellToUnHighlight.UnhighlightCell();
            HighlightedCellViewModels.Clear();
        }

        public void UpdateLayout()
        {
            var layout = new CellLayout(CellViewModels, _cellTracker);
            layout.UpdateLayout();
            SheetWidth = layout.LayoutWidth;
            SheetHeight = layout.LayoutHeight;
        }

        private void AddCellViewModel(CellModel newModel)
        {
            var newViewModel = CellViewModelFactory.Create(newModel, this);
            CellViewModels.Add(newViewModel);
            _cellModelToCellViewModelMap.Add(newModel, newViewModel);
            newViewModel.Model.PropertyChanged += CheckForCellTypePropertyChanged;
            UpdateLayout();
        }

        private void CellsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CellModel cell in e.NewItems!) AddCellViewModel(cell);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (CellModel cell in e.OldItems!) RemoveCellViewModel(cell);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                CellViewModels.Clear();
                foreach (var cell in _model.Cells) AddCellViewModel(cell);
            }
        }

        private void CheckForCellTypePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(CellModel.CellType)) return;
            var model = (CellModel)sender!;
            var viewModel = CellViewModels.FirstOrDefault(x => x.Model == model);
            if (viewModel == null) return;
            ReinstantiateCellsViewModel(viewModel);
        }

        private void EnsureUnmerged(CellModel cell)
        {
            if (!cell.IsMergedParent()) return;
            var cellEditor = new CellFormatEditWindowViewModel([cell], _cellTracker, _pluginFunctionLoader);
            cellEditor.UnmergeCells();
        }

        private void RemoveCellViewModel(CellModel cell)
        {
            EnsureUnmerged(cell);
            cell.PropertyChanged -= CheckForCellTypePropertyChanged;
            var viewModel = CellViewModels.First(x => x.Model == cell);
            CellViewModels.Remove(viewModel);
            HighlightedCellViewModels.Remove(viewModel);
            _cellModelToCellViewModelMap.Remove(cell);
        }

        private void SelectedCellsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (CellModel cell in e.NewItems!)
                {
                    if (_cellModelToCellViewModelMap.TryGetValue(cell, out var cellViewModel))
                    {
                        cellViewModel.IsSelected = true;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (CellModel cell in e.OldItems!)
                {
                    if (_cellModelToCellViewModelMap.TryGetValue(cell, out var cellViewModel))
                    {
                        cellViewModel.IsSelected = false;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var cell in CellViewModels.ToList()) UnselectCellViewModel(cell);
            }
        }

        private void UnselectCellViewModel(CellViewModel cell)
        {
            if (!cell.IsSelected) return;
            cell.IsSelected = false;
            if (SelectedCellViewModel == cell) SelectedCellViewModel = null;
        }
    }
}
