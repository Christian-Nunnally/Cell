using Cell.Data;
using Cell.Model;

namespace Cell.ViewModel
{
    public class CellClipboard
    {
        private CellModel? _centerOfCopy;
        private IEnumerable<CellModel> _clipboard = [];
        private bool _copyTextOnly = false;

        public void CopySelectedCells(SheetViewModel activeSheet, bool copyTextOnly)
        {
            _copyTextOnly = copyTextOnly;
            _centerOfCopy = activeSheet.SelectedCellViewModel?.Model;
            _clipboard = activeSheet.SelectedCellViewModels.Select(c => c.Model).ToList();
        }

        public void PasteCopiedCells(SheetViewModel activeSheet)
        {
            var pasteIntoCell = activeSheet.SelectedCellViewModel;
            if (pasteIntoCell is null) return;
            if (_clipboard is null) return;
            if (_centerOfCopy is null) return;
            if (!_clipboard.Any()) return;

            if (_copyTextOnly)
            {
                foreach (var cellToPaste in _clipboard)
                {
                    PasteCopiedCellTextOnly(activeSheet, pasteIntoCell, cellToPaste, _centerOfCopy);
                }
            }
            else
            {
                foreach (var cellToPaste in _clipboard)
                {
                    PasteCopiedCell(activeSheet, pasteIntoCell, cellToPaste, _centerOfCopy);
                }
                activeSheet.UpdateLayout();
            }
        }

        private static void PasteCopiedCell(SheetViewModel activeSheet, CellViewModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy)
        {
            var newRow = pasteIntoCell.Row + cellToPaste.Row - centerOfCopy.Row;
            var newColumn = pasteIntoCell.Column + cellToPaste.Column - centerOfCopy.Column;
            var cellToReplace = Cells.GetCell(pasteIntoCell.Model.SheetName, newRow, newColumn);
            if (cellToReplace is null) return;
            if (cellToReplace.CellType.IsSpecial()) return;

            var pastedCell = CopyCellWithUpdatedLocationProperties(newRow, newColumn, cellToReplace, cellToPaste);
            activeSheet.DeleteCell(cellToReplace);
            activeSheet.AddCell(pastedCell);
        }

        private static void PasteCopiedCellTextOnly(SheetViewModel activeSheet, CellViewModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy)
        {
            var newRow = pasteIntoCell.Row + cellToPaste.Row - centerOfCopy.Row;
            var newColumn = pasteIntoCell.Column + cellToPaste.Column - centerOfCopy.Column;
            var cellToReplace = Cells.GetCell(pasteIntoCell.Model.SheetName, newRow, newColumn);
            if (cellToReplace is null) return;
            if (cellToReplace.CellType.IsSpecial()) return;

            cellToReplace.Text = cellToPaste.Text;
        }

        private static CellModel CopyCellWithUpdatedLocationProperties(int newRow, int newColumn, CellModel cellToReplace, CellModel cellToPaste)
        {
            var pastedCell = cellToPaste.Copy();
            pastedCell.SheetName = cellToReplace.SheetName;
            pastedCell.Width = cellToReplace.Width;
            pastedCell.Height = cellToReplace.Height;
            pastedCell.Row = newRow;
            pastedCell.Column = newColumn;
            return pastedCell;
        }
    }
}
