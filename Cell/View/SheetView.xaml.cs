using Cell.Data;
using Cell.Model;
using Cell.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View
{
    /// <summary>
    /// Interaction logic for SheetView.xaml
    /// </summary>
    public partial class SheetView : UserControl
    {
        private bool _selectingCells = false;
        private CellViewModel? _selectionStart;

        public PanAndZoomCanvas? PanAndZoomCanvas;

        public SheetViewModel SheetViewModel => DataContext as SheetViewModel ?? SheetViewModel.NullSheet;

        public SheetView()
        {
            InitializeComponent();
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
                        SheetViewModel.UnselectAllCells();
                    }
                    SheetViewModel.SelectCell(cell);
                    _selectingCells = true;
                    _selectionStart = cell;
                    if (wasSelected)
                    {
                        if (cell.Model.CellType == CellType.Row)
                        {
                            foreach (var rowCell in Cells.GetCellModelsForSheet(SheetViewModel.SheetName).Where(x => x.Row == cell.Model.Row))
                            {
                                if (rowCell == cell.Model) continue;
                                SheetViewModel.SelectCell(rowCell);
                            }
                            SheetViewModel.UnselectCell(cell);
                        }
                        else if (cell.Model.CellType == CellType.Column)
                        {
                            foreach (var columnCell in Cells.GetCellModelsForSheet(SheetViewModel.SheetName).Where(x => x.Column == cell.Model.Column))
                            {
                                if (columnCell == cell.Model) continue;
                                SheetViewModel.SelectCell(columnCell);
                            }
                            SheetViewModel.UnselectCell(cell);
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
            if (e.LeftButton == MouseButtonState.Pressed && _selectingCells)
            {
                if (sender is FrameworkElement element)
                {
                    if (element.DataContext is CellViewModel cell)
                    {
                        if (Keyboard.GetKeyStates(Key.LeftCtrl) == KeyStates.Down || Keyboard.GetKeyStates(Key.RightCtrl) == KeyStates.Down)
                        {
                            if (CanSelectCell(cell.Model)) SheetViewModel.SelectCell(cell);
                        }
                        else
                        {
                            SheetViewModel.UnselectAllCells();
                            var startColumn = Math.Min(_selectionStart!.Column, cell.Column);
                            var endColumn = Math.Max(_selectionStart!.Column, cell.Column);
                            var startRow = Math.Min(_selectionStart!.Row, cell.Row);
                            var endRow = Math.Max(_selectionStart!.Row, cell.Row);
                            for (var row = startRow; row <= endRow; row++)
                            {
                                for (var column = startColumn; column <= endColumn; column++)
                                {
                                    var cellToSelect = Cells.GetCell(SheetViewModel.SheetName, row, column);
                                    if (CanSelectCell(cellToSelect)) SheetViewModel.SelectCell(cellToSelect!);
                                }
                            }
                            var topLeftCell = Cells.GetCell(SheetViewModel.SheetName, startRow, startColumn);
                            if (topLeftCell is not null) SheetViewModel.SelectCell(topLeftCell);
                        }
                    }
                }
            }
        }

        private bool CanSelectCell(CellModel? cell)
        {
            if (cell is null) return false;
            if (cell.CellType.IsSpecial() && IsSelectingSpecialCellsAllowed()) return false;
            if (!cell.CellType.IsSpecial() && IsSelectingNonSpecialCellsAllowed()) return false;
            return true;

            bool IsSelectingSpecialCellsAllowed() => SheetViewModel.SelectedCellViewModels.Where(x => x is not SpecialCellViewModel).Any();
            bool IsSelectingNonSpecialCellsAllowed() => SheetViewModel.SelectedCellViewModels.OfType<SpecialCellViewModel>().Any();
        }

        private void CellPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _selectingCells = false;
        }

        private void PanZoomCanvasLoaded(object sender, RoutedEventArgs e)
        {
            PanAndZoomCanvas = sender as PanAndZoomCanvas;
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox)
            {
                textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void TextBoxKeyDownForCell(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textbox) return;
            if (e.Key == Key.Enter)
            {
                textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                if (Keyboard.Modifiers == ModifierKeys.Shift) SheetViewModel?.MoveSelectionUp();
                else SheetViewModel?.MoveSelectionDown();
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                if (Keyboard.Modifiers == ModifierKeys.Shift) SheetViewModel?.MoveSelectionLeft();
                else SheetViewModel?.MoveSelectionRight();
                e.Handled = true;
            }
        }

        private void CellTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.DataContext is TextboxCellViewModel cell)
                {
                    cell.SetTextBox(textBox);
                }
            }
        }
    }
}
