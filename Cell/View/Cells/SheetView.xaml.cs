using Cell.Model;
using Cell.View.Application;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cell.View.Cells
{
    public partial class SheetView : UserControl
    {
        private CellViewModel? _currentCellMouseIsOver;
        private PanAndZoomCanvas? _panAndZoomCanvas;
        private bool _selectingCells = false;
        private CellViewModel? _selectionStart;
        /// <summary>
        /// Creates a new instance of <see cref="SheetView"/>.
        /// </summary>
        /// <param name="sheetViewModel">The view model for this view.</param>
        public SheetView(SheetViewModel sheetViewModel)
        {
            DataContext = sheetViewModel;
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets whether the canvas is locked to the center of the screen. which results in it snapping to the center when the window size changes.
        /// </summary>
        public bool IsLockedToCenter
        {
            get => _panAndZoomCanvas?.IsLockedToCenter ?? true;
            set
            {
                if (_panAndZoomCanvas != null) _panAndZoomCanvas.IsLockedToCenter = value;
            }
        }

        /// <summary>
        /// Gets or sets whether panning is enabled on the canvas.
        /// </summary>
        public bool IsPanningEnabled
        {
            get => _panAndZoomCanvas?.IsPanningEnabled ?? false;
            set
            {
                if (_panAndZoomCanvas != null)
                {
                    _panAndZoomCanvas.IsPanningEnabled = value;
                }
            }
        }

        /// <summary>
        /// Gets the view model for the sheet.
        /// </summary>
        public SheetViewModel SheetViewModel => DataContext as SheetViewModel ?? SheetViewModel.NullSheet;

        /// <summary>
        /// Pans the canvas to the specified location.
        /// </summary>
        /// <param name="x">The x sheet coordinate to pan to.</param>
        /// <param name="y">The y sheet coordinate to pan to.</param>
        public void PanCanvasTo(double x, double y)
        {
            _panAndZoomCanvas?.PanCanvasTo(x, y);
        }

        private bool CanSelectCell(CellModel? cell)
        {
            if (cell is null) return false;
            if (cell.CellType.IsSpecial() && IsSelectingSpecialCellsAllowed()) return false;
            if (!cell.CellType.IsSpecial() && IsSelectingNonSpecialCellsAllowed()) return false;
            return true;

            bool IsSelectingSpecialCellsAllowed() => SheetViewModel.CellSelector.SelectedCells.Where(x => !x.CellType.IsSpecial()).Any();
            bool IsSelectingNonSpecialCellsAllowed() => SheetViewModel.CellSelector.SelectedCells.Where(x => x.CellType.IsSpecial()).Any();
        }

        private void CellPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    var wasSelected = cell.IsSelected;
                    if (!(Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down || Keyboard.GetKeyStates(Key.RightCtrl) == KeyStates.Down))
                    {
                        SheetViewModel.CellSelector.UnselectAllCells();
                    }
                    SheetViewModel.CellSelector.SelectCell(cell.Model);
                    _selectingCells = true;
                    _selectionStart = cell;
                    if (wasSelected)
                    {
                        if (cell.Model.CellType == CellType.Row)
                        {
                            foreach (var rowCell in SheetViewModel.CellTracker.GetCellModelsForSheet(SheetViewModel.SheetName).Where(x => x.Location.Row == cell.Model.Location.Row))
                            {
                                if (rowCell == cell.Model) continue;
                                SheetViewModel.CellSelector.SelectCell(rowCell);
                            }
                            SheetViewModel.CellSelector.UnselectCell(cell.Model);
                        }
                        else if (cell.Model.CellType == CellType.Column)
                        {
                            foreach (var columnCell in SheetViewModel.CellTracker.GetCellModelsForSheet(SheetViewModel.SheetName).Where(x => x.Location.Column == cell.Model.Location.Column))
                            {
                                if (columnCell == cell.Model) continue;
                                SheetViewModel.CellSelector.SelectCell(columnCell);
                            }
                            SheetViewModel.CellSelector.UnselectCell(cell.Model);
                        }
                    }
                }
                else if (e.ChangedButton == MouseButton.Middle)
                {
                    ApplicationViewModel.Instance.GoToCell(cell.Model);
                }
            }
        }

        public void SetBackgroundColor(Brush brush)
        {
            Background = brush;
        }

        private void CellPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext(sender, out CellViewModel? cell) && cell is not null)
            {
                if (_currentCellMouseIsOver != cell)
                {
                    if (e.LeftButton == MouseButtonState.Pressed && _selectingCells)
                    {
                        if (Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down || Keyboard.GetKeyStates(Key.RightCtrl) == KeyStates.Down)
                        {
                            if (CanSelectCell(cell.Model)) SheetViewModel.CellSelector.SelectCell(cell.Model);
                        }
                        else
                        {
                            SheetViewModel?.CellSelector.UnselectAllCells();
                            var startColumn = Math.Min(_selectionStart!.Column, cell.Column);
                            var endColumn = Math.Max(_selectionStart!.Column, cell.Column);
                            var startRow = Math.Min(_selectionStart!.Row, cell.Row);
                            var endRow = Math.Max(_selectionStart!.Row, cell.Row);
                            for (var row = startRow; row <= endRow; row++)
                            {
                                for (var column = startColumn; column <= endColumn; column++)
                                {
                                    var cellToSelect = SheetViewModel?.CellTracker.GetCell(SheetViewModel.SheetName, row, column);
                                    if (CanSelectCell(cellToSelect)) SheetViewModel?.CellSelector.SelectCell(cellToSelect!);
                                }
                            }
                            var topLeftCell = SheetViewModel?.CellTracker.GetCell(SheetViewModel.SheetName, startRow, startColumn);
                            if (topLeftCell is not null) SheetViewModel?.CellSelector.SelectCell(topLeftCell);
                        }
                    }
                    else
                    {
                        if (SheetViewModel.IsCellHighlightOnMouseOverEnabled) SheetViewModel.HighlightCell(cell, "#33333333");
                    }
                }
                _currentCellMouseIsOver = cell;
            }
        }

        private void CellPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _selectingCells = false;
        }

        private void PanZoomCanvasLoaded(object sender, RoutedEventArgs e)
        {
            _panAndZoomCanvas = sender as PanAndZoomCanvas;
            if (_panAndZoomCanvas is null) return;
            _panAndZoomCanvas.LaidOutWidth = SheetViewModel.SheetWidth;
            _panAndZoomCanvas.LaidOutHeight = SheetViewModel.SheetHeight;
            SheetViewModel.PropertyChanged += SheetViewModelPropertyChanged;
            _panAndZoomCanvas.Background = Background;
            _panAndZoomCanvas.EnsureCenteredIfLocked();
        }

        private void SheetViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_panAndZoomCanvas is null) return;
            if (e.PropertyName == nameof(SheetViewModel.SheetWidth)) _panAndZoomCanvas.LaidOutWidth = SheetViewModel.SheetWidth;
            else if (e.PropertyName == nameof(SheetViewModel.SheetHeight)) _panAndZoomCanvas.LaidOutHeight = SheetViewModel.SheetHeight;
            if (e.PropertyName == nameof(SheetViewModel.PanX))
            {
                _panAndZoomCanvas?.PanCanvasTo(SheetViewModel.PanX, _panAndZoomCanvas.YPan);
            }
            else if (e.PropertyName == nameof(SheetViewModel.PanY))
            {
                _panAndZoomCanvas?.PanCanvasTo(_panAndZoomCanvas.XPan, SheetViewModel.PanY);
            }
            else if (e.PropertyName == nameof(SheetViewModel.IsPanningEnabled))
            {
                _panAndZoomCanvas.IsPanningEnabled = SheetViewModel.IsPanningEnabled;
            }
            else if (e.PropertyName == nameof(SheetViewModel.IsPanningEnabled))
            {
                _panAndZoomCanvas.IsLockedToCenter = SheetViewModel.IsLockedToCenter;
            }
        }

        private void CellMouseLeave(object sender, MouseEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext(sender, out CellViewModel? cell) && cell is not null)
            {
                SheetViewModel.UnhighlightAllCells();
            }
        }
    }
}
