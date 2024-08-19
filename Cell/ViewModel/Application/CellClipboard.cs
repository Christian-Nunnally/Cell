using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.ViewModel.Cells;
using System.Diagnostics.CodeAnalysis;

namespace Cell.ViewModel.Application
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

            if (_clipboard.Count() == 1)
            {
                var cellToPaste = _clipboard.First();
                foreach (var cell in activeSheet.SelectedCellViewModels.ToList())
                {
                    if (_copyTextOnly) cell.Text = cellToPaste.Text;
                    else PasteSingleCell(cellToPaste, cell.Model);
                }
            }
            else
            {
                foreach (var cellToPaste in _clipboard)
                {
                    if (_copyTextOnly) PasteCopiedCellTextOnly(pasteIntoCell, cellToPaste, _centerOfCopy);
                    else PasteCopiedCell(pasteIntoCell, cellToPaste, _centerOfCopy);
                }
            }
            activeSheet.UpdateLayout();
        }

        private static void PasteCopiedCell(CellViewModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy)
        {
            if (!TryGetCellToReplace(pasteIntoCell, cellToPaste, centerOfCopy, out var cellToReplace)) return;
            PasteSingleCell(cellToPaste, cellToReplace);
        }

        private static void PasteCopiedCellTextOnly(CellViewModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy)
        {
            if (!TryGetCellToReplace(pasteIntoCell, cellToPaste, centerOfCopy, out var cellToReplace)) return;
            UndoRedoManager.RecordCellStateForUndo(cellToReplace);
            cellToReplace.Text = cellToPaste.Text;
        }

        private static void PasteSingleCell(CellModel cellToPaste, CellModel cellToReplace)
        {
            UndoRedoManager.RecordCellStateForUndo(cellToReplace);
            cellToPaste.CopyProperties(cellToReplace, [nameof(CellModel.ID), nameof(CellModel.SheetName), nameof(CellModel.Width), nameof(CellModel.Height), nameof(CellModel.Row), nameof(CellModel.Column), nameof(CellModel.MergedWith), nameof(CellModel.Value), nameof(CellModel.Date)]);
        }

        private static bool TryGetCellToReplace(CellViewModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy, [MaybeNullWhen(false)] out CellModel cellToReplace)
        {
            var newRow = pasteIntoCell.Row + cellToPaste.Row - centerOfCopy.Row;
            var newColumn = pasteIntoCell.Column + cellToPaste.Column - centerOfCopy.Column;
            cellToReplace = CellTracker.Instance.GetCell(pasteIntoCell.Model.SheetName, newRow, newColumn);
            if (cellToReplace is null) return false;
            if (cellToReplace.CellType.IsSpecial()) return false;
            return true;
        }
    }
}
