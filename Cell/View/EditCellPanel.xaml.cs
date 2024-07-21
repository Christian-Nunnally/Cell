﻿using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            if (Utilities.TryGetSendersDataContext<ColumnCellViewModel>(sender, out var cell))
            {
                cell.AddColumnToTheLeft();
            }
        }

        private void CreateNewColumnToTheRightButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<ColumnCellViewModel>(sender, out var cell))
            {
                cell.AddColumnToTheRight();
            }
        }

        private void CreateNewRowAboveButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<RowCellViewModel>(sender, out var cell))
            {
                cell.AddRowAbove();
            }
        }

        private void CreateNewRowBelowButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<RowCellViewModel>(sender, out var cell))
            {
                cell.AddRowBelow();
            }
        }
        private void CellTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1 && e.AddedItems[0] is Label label && Enum.TryParse(typeof(CellType), label.Content.ToString(), out var newType) && newType is CellType cellType)
            {
                ApplicationViewModel.Instance.ChangeSelectedCellsType(cellType);
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
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                if (string.IsNullOrEmpty(cell.PopulateFunctionName)) cell.PopulateFunctionName = "Untitled";
                CodeEditorViewModel.Show(cell.PopulateFunctionCode, x => cell.PopulateFunctionCode = x, true, cell);
            }
        }

        private void EditOnEditFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                if (string.IsNullOrEmpty(cell.TriggerFunctionName)) cell.TriggerFunctionName = "Untitled";
                CodeEditorViewModel.Show(cell.TriggerFunctionCode, x => cell.TriggerFunctionCode = x, false, cell);
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
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                cell.HorizontalAlignmentForView = HorizontalAlignment.Left;
                cell.VerticalAlignmentForView = VerticalAlignment.Top;
            }
        }

        private void SetAlignmentToTopButtonClick(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                cell.HorizontalAlignmentForView = HorizontalAlignment.Stretch;
                cell.VerticalAlignmentForView = VerticalAlignment.Top;
            }
        }
            
        private void SetAlignmentToTopRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                cell.HorizontalAlignmentForView = HorizontalAlignment.Right;
                cell.VerticalAlignmentForView = VerticalAlignment.Top;
            }
        }

        private void SetAlignmentToLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                cell.HorizontalAlignmentForView = HorizontalAlignment.Left;
                cell.VerticalAlignmentForView = VerticalAlignment.Stretch;
            }
        }

        private void SetAlignmentToCenterButtonClick(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                cell.HorizontalAlignmentForView = HorizontalAlignment.Center;
                cell.VerticalAlignmentForView = VerticalAlignment.Center;
            }
        }

        private void SetAlignmentToRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                cell.HorizontalAlignmentForView = HorizontalAlignment.Right;
                cell.VerticalAlignmentForView = VerticalAlignment.Stretch;
            }
        }

        private void SetAlignmentToBottomLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                cell.HorizontalAlignmentForView = HorizontalAlignment.Left;
                cell.VerticalAlignmentForView = VerticalAlignment.Bottom;
            }
        }

        private void SetAlignmentToBottomButtonClick(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                cell.HorizontalAlignmentForView = HorizontalAlignment.Stretch;
                cell.VerticalAlignmentForView = VerticalAlignment.Bottom;
            }
        }

        private void SetAlignmentToBottomRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                cell.HorizontalAlignmentForView = HorizontalAlignment.Right;
                cell.VerticalAlignmentForView = VerticalAlignment.Bottom;
            }
        }

        private void SetTextAlignmentToLeftButtonClick(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                cell.TextAlignmentForView = TextAlignment.Left;
            }
        }

        private void SetTextAlignmentToCenterButtonClick(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                cell.TextAlignmentForView = TextAlignment.Center;
            }
        }

        private void SetTextAlignmentToRightButtonClick(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                cell.TextAlignmentForView = TextAlignment.Right;
            }
        }

        private void IndexButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Utilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
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
        }
    }
}
