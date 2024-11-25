using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Execution;
using Cell.Model;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Cell.Core.Data.Tracker;
using System.Windows.Threading;
using Cell.ViewModel.Application;

namespace Cell.ViewModel.Cells
{
    /// <summary>
    /// A view model for a sheet in the application.
    /// </summary>
    public class SheetViewModel : PropertyChangedBase
    {
        /// <summary>
        /// A null sheet that can be used as a placeholder.
        /// </summary>
        public static readonly SheetViewModel NullSheet = new(SheetModel.Null, null!, null!, null!, CellSelector.Null, null!, null!);
        private readonly Dictionary<CellModel, CellViewModel> _cellModelToCellViewModelMap = [];
        private readonly CellPopulateManager _cellPopulateManager;
        private readonly CellTriggerManager _cellTriggerManager;
        private readonly CellTracker _cellTracker;
        private readonly SheetModel _model;
        private readonly FunctionTracker _functionTracker;
        private double _sheetHeight;
        private double _sheetWidth;
        private double _panX;
        private double _panY;
        private bool _isPanningEnabled;
        private bool _isLockedToCenter;
        private UndoRedoManager _undoRedoManager;
        private CellLayout _layout;

        /// <summary>
        /// Creates a new instance of <see cref="SheetViewModel"/>.
        /// </summary>
        /// <param name="model">The sheet model for this sheet.</param>
        /// <param name="cellPopulateManager">The cell populate manager used to run populate functions on cells.</param>
        /// <param name="cellTriggerManager">The cell trigger manager used to run trigger functions on cells.</param>
        /// <param name="cellTracker">The cell tracker used to get cells for this sheet.</param>
        /// <param name="cellSelector">The cell selector used to select cells on the sheet.</param>
        /// <param name="undoRedoManager">The undo redo manager used by this sheet.</param>
        /// <param name="functionTracker">Used to get cell functions from the application.</param>
        public SheetViewModel(
            SheetModel model,
            CellPopulateManager cellPopulateManager,
            CellTriggerManager cellTriggerManager,
            CellTracker cellTracker,
            CellSelector cellSelector,
            UndoRedoManager undoRedoManager,
            FunctionTracker functionTracker)
        {
            _functionTracker = functionTracker;
            _cellTracker = cellTracker;
            CellSelector = cellSelector;
            cellSelector.SelectedCells.CollectionChanged += SelectedCellsChanged;
            _cellPopulateManager = cellPopulateManager;
            _cellTriggerManager = cellTriggerManager;
            _model = model;
            _undoRedoManager = undoRedoManager;
            _model.Cells.CollectionChanged += CellsCollectionChanged;
            _layout = new CellLayout(CellViewModels, _cellTracker);
            _layout.LayoutUpdated += HandleLayoutUpdated;
        }

        /// <summary>
        /// Creates cell view models for all the cells in the sheet.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeCellViewModelsAsync()
        {
            foreach (var cell in _model.Cells)
            {
                AddCellViewModel(cell);
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
            }
        }

        /// <summary>
        /// The populate manager used to run populate functions on cells.
        /// </summary>
        public CellPopulateManager CellPopulateManager => _cellPopulateManager;

        /// <summary>
        /// The cell trigger manager used to run trigger functions on cells.
        /// </summary>
        public CellTriggerManager CellTriggerManager => _cellTriggerManager;

        /// <summary>
        /// The cell selector used to select cells on the sheet.
        /// </summary>
        public CellSelector CellSelector { get; private set; }

        /// <summary>
        /// The cell tracker used to get cells for this sheet.
        /// </summary>
        public CellTracker CellTracker => _cellTracker;

        /// <summary>
        /// The collection of cell view models for this sheet.
        /// </summary>
        public ObservableCollection<CellViewModel> CellViewModels { get; set; } = [];

        /// <summary>
        /// The collection of cell view models that are currently highlighted.
        /// </summary>
        public List<CellViewModel> HighlightedCellViewModels { get; } = [];

        /// <summary>
        /// Enables or disables the cell highlight on mouse over feature.
        /// </summary>
        public bool IsCellHighlightOnMouseOverEnabled { get; internal set; } = true;

        /// <summary>
        /// The amount to pan the sheet on the x axis.
        /// </summary>
        public double PanX
        {
            get => _panX;
            set
            {
                if (_panX == value) return;
                _panX = value;
                NotifyPropertyChanged(nameof(PanX));
            }
        }

        /// <summary>
        /// The amount to pan the sheet on the y axis.
        /// </summary>
        public double PanY
        {
            get => _panY;
            set
            {
                if (_panY == value) return;
                _panY = value;
                NotifyPropertyChanged(nameof(PanY));
            }
        }

        /// <summary>
        /// The height of all the cells on the sheet combined.
        /// </summary>
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

        /// <summary>
        /// Gets whether panning is enabled on the canvas.
        /// </summary>
        public bool IsPanningEnabled
        {
            get => _isPanningEnabled;
            set
            {
                if (_isPanningEnabled == value) return;
                _isPanningEnabled = value;
                NotifyPropertyChanged(nameof(IsPanningEnabled));
            }
        }

        /// <summary>
        /// Gets or sets whether the canvas is locked to the center of the screen. which results in it snapping to the center when the window size changes.
        /// </summary>
        public bool IsLockedToCenter
        {
            get => _isLockedToCenter;
            set
            {
                if (_isLockedToCenter == value) return;
                _isLockedToCenter = value;
                NotifyPropertyChanged(nameof(IsLockedToCenter));
            }
        }

        /// <summary>
        /// Gets the name of the sheet.
        /// </summary>
        public string SheetName => _model.Name;

        /// <summary>
        /// The width of all the cells on the sheet combined.
        /// </summary>
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

        /// <summary>
        /// Highlights the given cell with the given color.
        /// </summary>
        /// <param name="cellToHighlight">The cell to highlight.</param>
        /// <param name="color">The color to highlight the cell with.</param>
        public void HighlightCell(CellViewModel cellToHighlight, string color)
        {
            cellToHighlight.HighlightCell(color);
            HighlightedCellViewModels.Add(cellToHighlight);
        }

        /// <summary>
        /// Deletes the given cell view model and creates a new one based on the models new type.
        /// </summary>
        /// <param name="cellViewModel">The view model to replace.</param>
        public void ReinstantiateCellsViewModel(CellViewModel cellViewModel)
        {
            RemoveCellViewModel(cellViewModel.Model);
            AddCellViewModel(cellViewModel.Model);
        }

        /// <summary>
        /// Removes all highlights from cells on the sheet.
        /// </summary>
        public void UnhighlightAllCells()
        {
            foreach (var cellToUnHighlight in HighlightedCellViewModels) cellToUnHighlight.UnhighlightCell();
            HighlightedCellViewModels.Clear();
        }

        /// <summary>
        /// Recalculates the layout of the cells on the sheet.
        /// </summary>
        public void HandleLayoutUpdated()
        {
            SheetWidth = _layout.LayoutWidth;
            SheetHeight = _layout.LayoutHeight;
        }

        private void AddCellViewModel(CellModel newModel)
        {
            var newViewModel = CellViewModelFactory.Create(newModel, this);
            CellViewModels.Add(newViewModel);
            _cellModelToCellViewModelMap.Add(newModel, newViewModel);
            newViewModel.Model.PropertyChanged += CheckForCellTypePropertyChanged;
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
            if (viewModel is null) return;
            ReinstantiateCellsViewModel(viewModel);
        }

        private void EnsureUnmerged(CellModel cell)
        {
            if (!cell.IsMergedParent()) return;
            var cellEditor = new CellFormatEditWindowViewModel([cell], _cellTracker, _functionTracker, _undoRedoManager);
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
                foreach (var cell in CellViewModels.ToList()) cell.IsSelected = false;
            }
        }
    }
}
