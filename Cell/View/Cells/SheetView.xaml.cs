using Cell.Model;
using Cell.View.Application;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.Cells
{
    /// <summary>
    /// Interaction logic for SheetView.xaml
    /// </summary>
    public partial class SheetView : UserControl
    {
        private PanAndZoomCanvas? _panAndZoomCanvas;
        private CellViewModel? _currentCellMouseIsOver;
        private bool _selectingCells = false;
        private CellViewModel? _selectionStart;
        public SheetView()
        {
            InitializeComponent();
        }

        public SheetViewModel SheetViewModel => DataContext as SheetViewModel ?? SheetViewModel.NullSheet;

        public bool IsPanningEnabled 
        { 
            get => _panAndZoomCanvas?.IsPanningEnabled ?? false;
            set 
            {
                if (_panAndZoomCanvas != null) _panAndZoomCanvas.IsPanningEnabled = value;
            } 
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
                            foreach (var rowCell in SheetViewModel.CellTracker.GetCellModelsForSheet(SheetViewModel.SheetName).Where(x => x.Row == cell.Model.Row))
                            {
                                if (rowCell == cell.Model) continue;
                                SheetViewModel.CellSelector.SelectCell(rowCell);
                            }
                            SheetViewModel.CellSelector.UnselectCell(cell.Model);
                        }
                        else if (cell.Model.CellType == CellType.Column)
                        {
                            foreach (var columnCell in SheetViewModel.CellTracker.GetCellModelsForSheet(SheetViewModel.SheetName).Where(x => x.Column == cell.Model.Column))
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
                        SheetViewModel.UnhighlightAllCells();
                        SheetViewModel.HighlightCell(cell, "#33333333");
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
            if (_panAndZoomCanvas == null) return;
        }

        public void PanCanvasTo(double x, double y)
        {
            _panAndZoomCanvas?.PanCanvasTo(x, y);
        }

        internal void ZoomCanvasTo(Point point, double zoom)
        {
            _panAndZoomCanvas?.ZoomCanvasTo(point, zoom);
        }
    }
}
