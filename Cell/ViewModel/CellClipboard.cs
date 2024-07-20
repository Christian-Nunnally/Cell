using Cell.Model;
using Cell.Persistence;

namespace Cell.ViewModel
{
    public class CellClipboard
    {
        private CellModel? _centerOfCopy;
        private IEnumerable<CellModel> _clipboard = [];

        public void CopySelectedCells(SheetViewModel activeSheet)
        {
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

            foreach (var cellToPaste in _clipboard)
            {
                var newRow = pasteIntoCell.Row + cellToPaste.Row - _centerOfCopy.Row;
                var newColumn = pasteIntoCell.Column + cellToPaste.Column - _centerOfCopy.Column;
                var cellToReplace = Cells.GetCell(cellToPaste.SheetName, newRow, newColumn);
                if (cellToReplace is null) continue;
                if (cellToReplace.CellType.IsSpecial()) continue;
                var pastedCell = cellToPaste.Copy();
                pastedCell.Width = cellToReplace.Width;
                pastedCell.Height = cellToReplace.Height;
                pastedCell.Row = newRow;
                pastedCell.Column = newColumn;

                activeSheet.DeleteCell(cellToReplace);
                activeSheet.AddCell(pastedCell);
            }
            activeSheet.UpdateLayout();
        }
    }
}
