using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using Cell.View.Converters;
using Cell.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace Cell.View
{
    /// <summary>
    /// Interaction logic for EditCellPanel.xaml
    /// </summary>
    public partial class EditCellPanel : UserControl
    {
        public EditCellPanel()
        {
            InitializeComponent();
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
        private void CellTypeComboBoxSelectionChanged(object sender, EventArgs e)
        {
            if (sender is not ComboBox comboBox) return;
            if (comboBox.SelectedValue is not Label label) return;
            var cellTypeString = label.Content.ToString();
            if (Enum.TryParse(cellTypeString, out CellType newType))
            {
                ApplicationViewModel.Instance.ChangeSelectedCellsType(newType);
            }
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {

        }

        private void DeleteRowButtonClicked(object sender, RoutedEventArgs e)
        {
            foreach (var cell in ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.OfType<RowCellViewModel>().ToList())
            {
                ApplicationViewModel.Instance.SheetViewModel.UnselectAllCells();
                cell.DeleteRow();
            }
        }

        private void DeleteColumnButtonClicked(object sender, RoutedEventArgs e)
        {
            foreach (var cell in ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.OfType<ColumnCellViewModel>().ToList())
            {
                ApplicationViewModel.Instance.SheetViewModel.UnselectAllCells();
                cell.DeleteColumn();
            }
        }

        private void EditGetTextFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                if (string.IsNullOrEmpty(cell.PopulateFunctionName)) cell.PopulateFunctionName = "Untitled";
                var function = PluginFunctionLoader.GetOrCreateFunction("object", cell.PopulateFunctionName);
                CodeEditorViewModel.Show(function.GetUserFriendlyCode(cell.Model), x => {
                    function.SetUserFriendlyCode(x, cell.Model);
                    (cell as ListCellViewModel)?.UpdateList();
                }, true, cell);
            }
        }

        private void EditOnEditFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                if (string.IsNullOrEmpty(cell.TriggerFunctionName)) cell.TriggerFunctionName = "Untitled";
                var function = PluginFunctionLoader.GetOrCreateFunction("void", cell.TriggerFunctionName);
                CodeEditorViewModel.Show(function.GetUserFriendlyCode(cell.Model), x => function.SetUserFriendlyCode(x, cell.Model), false, cell);
            }
        }

        private void CreateBackupButtonClicked(object sender, RoutedEventArgs e)
        {
            PersistenceManager.CreateBackup();
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void MergeButtonClicked(object sender, RoutedEventArgs e)
        {
            var selectedCells = ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.ToList();
            MergeCells(selectedCells);
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
            if (cellsToMerge.Any(cell => cell.IsMerged())) return;
            SetMergedWithToCellsId(cellsToMerge, topLeftCell);
            ApplicationViewModel.Instance.SheetViewModel.UpdateLayout();
        }

        private static void SetMergedWithToCellsId(List<CellModel> cellsToMerge, CellViewModel topLeftCell)
        {
            foreach (var cell in cellsToMerge) cell.MergedWith = topLeftCell.ID;
        }

        private static List<CellModel> GetCellsInRectangle(int startRow, int startColumn, int endRow, int endColumn, string sheetName)
        {
            var cells = new List<CellModel>();
            for (var row = startRow; row <= endRow; row++)
            {
                for (var column = startColumn; column <= endColumn; column++)
                {
                    var cell = Cells.Instance.GetCell(sheetName, row, column);
                    if (cell is not null) cells.Add(cell);
                }
            }
            return cells;
        }

        private void UnmergeButtonClicked(object sender, RoutedEventArgs e)
        {
            foreach (var selectedCell in ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.Where(x => x.ID == x.Model.MergedWith))
            {
                ApplicationViewModel.Instance.SheetViewModel.UnmergeCell(selectedCell);
            }
            ApplicationViewModel.Instance.SheetViewModel.UpdateLayout();
        }

        private void SetAlignmentToTopLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Left;
            cell.VerticalAlignmentForView = VerticalAlignment.Top;
        }

        private void SetAlignmentToTopButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Stretch;
            cell.VerticalAlignmentForView = VerticalAlignment.Top;
        }
            
        private void SetAlignmentToTopRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Right;
            cell.VerticalAlignmentForView = VerticalAlignment.Top;
        }

        private void SetAlignmentToLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Left;
            cell.VerticalAlignmentForView = VerticalAlignment.Stretch;
        }

        private void SetAlignmentToCenterButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Center;
            cell.VerticalAlignmentForView = VerticalAlignment.Center;
        }

        private void SetAlignmentToRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Right;
            cell.VerticalAlignmentForView = VerticalAlignment.Stretch;
        }

        private void SetAlignmentToBottomLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Left;
            cell.VerticalAlignmentForView = VerticalAlignment.Bottom;
        }

        private void SetAlignmentToBottomButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Stretch;
            cell.VerticalAlignmentForView = VerticalAlignment.Bottom;
        }

        private void SetAlignmentToBottomRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.HorizontalAlignmentForView = HorizontalAlignment.Right;
            cell.VerticalAlignmentForView = VerticalAlignment.Bottom;
        }

        private void SetTextAlignmentToLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.TextAlignmentForView = TextAlignment.Left;
        }

        private void SetTextAlignmentToCenterButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.TextAlignmentForView = TextAlignment.Center;
        }

        private void SetTextAlignmentToRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            cell.TextAlignmentForView = TextAlignment.Right;
        }

        private void IndexButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell)) return;
            var selectedCells = ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.ToList();
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

        private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not ColorPicker colorPicker) return;
            colorPicker.AvailableColorsSortingMode = ColorSortingMode.Alphabetical;
            colorPicker.AvailableColors.Clear();
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#9678b5"), "I"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#b272a1"), "Love"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#c17188"), "You"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#c3776f"), "Roxy"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#b8825c"), "<3"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#a48f54"), "You"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#8a9b5c"), "Can"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#6da471"), "Name"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#50aa8f"), "These"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#3dadaf"), "Colors"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#4aadca"), "Here"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#6fa9dc"), "!"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#d8d8d8"), "!"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#d0cece"), "!"));

            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#ebe5f1"), "!"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#f6dfec"), "!"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#fbdae1"), "!"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#fcdac5"), "!"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#edf5e0"), "!"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#e2f3ee"), "!"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#def3f8"), "!"));
            colorPicker.AvailableColors.Add(new ColorItem(RGBHexColorConverter.ConvertHexStringToColor("#deebf7"), "!"));
        }
    }
}
