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
        private void CellTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1 || e.AddedItems[0] is not Label label) return;
            var cellTypeString = label.Content.ToString();
            if (Enum.TryParse(cellTypeString, out CellType newType))
            {
                ApplicationViewModel.Instance.ChangeSelectedCellsType(newType);
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
                var function = PluginFunctionLoader.GetOrCreateFunction(PluginFunctionLoader.PopulateFunctionsDirectoryName, cell.PopulateFunctionName);
                CodeEditorViewModel.Show(function.Code, x => {
                    function.Code = x;
                    (cell as ListCellViewModel)?.UpdateList();
                }, false, cell);
            }
        }

        private void EditOnEditFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                if (string.IsNullOrEmpty(cell.TriggerFunctionName)) cell.TriggerFunctionName = "Untitled";
                var function = PluginFunctionLoader.GetOrCreateFunction(PluginFunctionLoader.TriggerFunctionsDirectoryName, cell.TriggerFunctionName);
                CodeEditorViewModel.Show(function.Code, x => function.Code = x, false, cell);
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
            var leftmost = selectedCells.Select(x => x.Column).Min();
            var topmost = selectedCells.Select(x => x.Row).Min();
            var rightmost = selectedCells.Select(x => x.Column).Max();
            var bottommost = selectedCells.Select(x => x.Row).Max();

            var topLeftCell = ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.FirstOrDefault(x => x.Row == topmost && x.Column == leftmost);
            if (topLeftCell is null) return;
            var bottomRightCell = ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.FirstOrDefault(x => x.Row == bottommost && x.Column == rightmost);
            if (bottomRightCell is null) return;

            var sheetName = ApplicationViewModel.Instance.SheetViewModel.SheetName;
            var cellsToMerge = new List<CellModel>();
            for (var r = topLeftCell.Row; r <= bottomRightCell.Row; r++)
            {
                for (var c = topLeftCell.Column; c <= bottomRightCell.Column; c++)
                {
                    var cell = Cells.GetCell(sheetName, r, c);
                    if (cell is not null)
                    {
                        if (!string.IsNullOrWhiteSpace(cell.MergedWith)) return;
                        cellsToMerge.Add(cell);
                    }
                }
            }
            foreach (var cell in cellsToMerge)
            {
                cell.MergedWith = topLeftCell.ID;
            }
            ApplicationViewModel.Instance.SheetViewModel.UpdateLayout();
        }

        private void UnmergeButtonClicked(object sender, RoutedEventArgs e)
        {
            foreach (var selectedCell in ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.Where(x => x.ID == x.Model.MergedWith))
            {
                if (selectedCell is null) return;
                var cells = ApplicationViewModel.Instance.SheetViewModel.CellViewModels.Where(x => x.Model.MergedWith == selectedCell.ID);
                foreach (var cell in cells)
                {
                    if (selectedCell == cell) continue;
                    cell.Model.MergedWith = string.Empty;
                }
                selectedCell.Model.MergedWith = string.Empty;
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
            foreach (var selectedCell in selectedCells)
            {
                if (selectedCell == topLeftCell) continue;
                var distance = (selectedCell.Column - topLeftCell.Column) + (selectedCell.Row - topLeftCell.Row);
                selectedCell.Index = topLeftCell.Index + distance;
            }
        }

        private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not ColorPicker colorPicker) return;
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
