using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types.Special;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace Cell.View.ToolWindow
{
    /// <summary>
    /// Interaction logic for EditCellPanel.xaml
    /// </summary>
    public partial class CellFormatEditWindow : UserControl, IToolWindow
    {
        public CellFormatEditWindow()
        {
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public static void AddColorsToColorPicker(ColorPicker colorPicker, List<string> colors, float brightnessFactor)
        {
            foreach (var color in colors)
            {
                var adjustedColor = ColorAdjuster.AdjustBrightness(color, brightnessFactor);
                colorPicker.AvailableColors.Add(new ColorItem(ColorAdjuster.ConvertHexStringToColor(adjustedColor), ""));
            }
        }

        public string GetTitle()
        {
            var currentlySelectedCell = ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModel;
            if (currentlySelectedCell is null) return "Select a cell to edit";
            if (currentlySelectedCell is CornerCellViewModel) return "Edit default sheet format";
            return $"Format editor - {currentlySelectedCell.GetName()}";
        }

        public List<CommandViewModel> GetToolBarCommands() => [
            //new CommandViewModel("void", new RelayCommand(x => true, x => {})),
            ];

        public bool HandleBeingClosed()
        {
            return true;
        }

        private static List<CellModel> GetCellsInRectangle(int startRow, int startColumn, int endRow, int endColumn, string sheetName)
        {
            var cells = new List<CellModel>();
            for (var row = startRow; row <= endRow; row++)
            {
                for (var column = startColumn; column <= endColumn; column++)
                {
                    var cell = ApplicationViewModel.Instance.CellTracker.GetCell(sheetName, row, column);
                    if (cell is not null) cells.Add(cell);
                }
            }
            return cells;
        }

        private static void MergeCells(List<CellViewModel> cells)
        {
            if (cells.Count < 2) return;
            var leftmost = cells.Select(x => x.Column).Min();
            var topmost = cells.Select(x => x.Row).Min();
            var rightmost = cells.Select(x => x.Column).Max();
            var bottommost = cells.Select(x => x.Row).Max();

            var topLeftCell = cells.FirstOrDefault(x => x.Row == topmost && x.Column == leftmost);
            if (topLeftCell is null) return;
            var bottomRightCell = cells.FirstOrDefault(x => x.Row == bottommost && x.Column == rightmost);
            if (bottomRightCell is null) return;

            var sheetName = topLeftCell.Model.SheetName;
            var cellsToMerge = GetCellsInRectangle(topmost, leftmost, bottommost, rightmost, sheetName);
            if (cellsToMerge.Count(cell => cell.IsMerged()) <= 1)
            {
                UnmergeSelectedCells();
            }
            else return;
            SetMergedWithToCellsId(cellsToMerge, topLeftCell);
            ApplicationViewModel.Instance.SheetViewModel.UpdateLayout();
        }

        private static void SetMergedWithToCellsId(List<CellModel> cellsToMerge, CellViewModel topLeftCell)
        {
            foreach (var cell in cellsToMerge) cell.MergedWith = topLeftCell.ID;
        }

        private static void UnmergeSelectedCells()
        {
            foreach (var selectedCell in ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.Where(x => x.ID == x.Model.MergedWith))
            {
                ApplicationViewModel.Instance.SheetViewModel.UnmergeCell(selectedCell);
            }
            ApplicationViewModel.Instance.SheetViewModel.UpdateLayout();
        }

        private void ChangeCellTypeCellClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            var cellTypeString = button.Content is Label label ? label.Content.ToString() : button.Content.ToString();
            if (Enum.TryParse(cellTypeString, out CellType newType))
            {
                ApplicationViewModel.Instance.ChangeSelectedCellsType(newType);
            }
        }

        private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not ColorPicker colorPicker) return;
            var colors = new List<string> { "#9678b5", "#b272a1", "#c17188", "#c3776f", "#b8825c", "#a48f54", "#8a9b5c", "#6da471", "#50aa8f", "#3dadaf", "#4aadca", "#6fa9dc" };
            colorPicker.AvailableColors.Clear();
            colorPicker.AvailableColorsSortingMode = ColorSortingMode.Alphabetical;
            AddColorsToColorPicker(colorPicker, colors, 1.0f);
            AddColorsToColorPicker(colorPicker, colors, .1f);
            AddColorsToColorPicker(colorPicker, colors, 1.9f);
        }

        private void CreateBackupButtonClicked(object sender, RoutedEventArgs e)
        {
            ApplicationViewModel.Instance.PersistenceManager.CreateBackup();
        }

        private void CreateNewColumnToTheLeftButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<ColumnCellViewModel>(sender, out var cell)) return;
            cell.AddColumnToTheLeft();
        }

        private void CreateNewColumnToTheRightButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<ColumnCellViewModel>(sender, out var cell)) return;
            cell.AddColumnToTheRight();
        }

        private void CreateNewRowAboveButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<RowCellViewModel>(sender, out var cell)) return;
            cell.AddRowAbove();
        }

        private void CreateNewRowBelowButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<RowCellViewModel>(sender, out var cell)) return;
            cell.AddRowBelow();
        }

        private void DeleteColumnButtonClicked(object sender, RoutedEventArgs e)
        {
            foreach (var cell in ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.OfType<ColumnCellViewModel>().ToList())
            {
                ApplicationViewModel.Instance.SheetViewModel.UnselectAllCells();
                cell.DeleteColumn();
            }
        }

        private void DeleteRowButtonClicked(object sender, RoutedEventArgs e)
        {
            foreach (var cell in ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.OfType<RowCellViewModel>().ToList())
            {
                ApplicationViewModel.Instance.SheetViewModel.UnselectAllCells();
                cell.DeleteRow();
            }
        }

        private void MergeAcrossButtonClicked(object sender, RoutedEventArgs e)
        {
            var selectedCells = ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.ToList();
            var rows = selectedCells.Select(x => x.Row).Distinct().ToList();
            foreach (var row in rows)
            {
                var cellsToMerge = selectedCells.Where(x => x.Row == row).ToList();
                MergeCells(cellsToMerge);
            }
        }

        private void MergeButtonClicked(object sender, RoutedEventArgs e)
        {
            var selectedCells = ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.ToList();
            MergeCells(selectedCells);
        }

        private void MergeDownButtonClicked(object sender, RoutedEventArgs e)
        {
            var selectedCells = ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.ToList();
            var columns = selectedCells.Select(x => x.Column).Distinct().ToList();
            foreach (var column in columns)
            {
                var cellsToMerge = selectedCells.Where(x => x.Column == column).ToList();
                MergeCells(cellsToMerge);
            }
        }

        private void SetAlignmentToBottomButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Stretch;
            cell.VerticalAlignmentForView = VerticalAlignment.Bottom;
        }

        private void SetAlignmentToBottomLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Left;
            cell.VerticalAlignmentForView = VerticalAlignment.Bottom;
        }

        private void SetAlignmentToBottomRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Right;
            cell.VerticalAlignmentForView = VerticalAlignment.Bottom;
        }

        private void SetAlignmentToCenterButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Center;
            cell.VerticalAlignmentForView = VerticalAlignment.Center;
        }

        private void SetAlignmentToLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Left;
            cell.VerticalAlignmentForView = VerticalAlignment.Stretch;
        }

        private void SetAlignmentToRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Right;
            cell.VerticalAlignmentForView = VerticalAlignment.Stretch;
        }

        private void SetAlignmentToTopButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Stretch;
            cell.VerticalAlignmentForView = VerticalAlignment.Top;
        }

        private void SetAlignmentToTopLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Left;
            cell.VerticalAlignmentForView = VerticalAlignment.Top;
        }

        private void SetAlignmentToTopRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Right;
            cell.VerticalAlignmentForView = VerticalAlignment.Top;
        }

        private void SetTextAlignmentToCenterButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.TextAlignmentForView = TextAlignment.Center;
        }

        private void SetTextAlignmentToLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.TextAlignmentForView = TextAlignment.Left;
        }

        private void SetTextAlignmentToRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.TextAlignmentForView = TextAlignment.Right;
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void UnmergeButtonClicked(object sender, RoutedEventArgs e)
        {
            UnmergeSelectedCells();
        }
    }
}
