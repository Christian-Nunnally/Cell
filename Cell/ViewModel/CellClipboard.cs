using Cell.Data;
using Cell.Model;
using System.Diagnostics.CodeAnalysis;

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

            if (_clipboard.Count() == 1)
            {
                var cellToPaste = _clipboard.First();
                foreach (var cell in activeSheet.SelectedCellViewModels.ToList())
                {
                    if (_copyTextOnly) cell.Text = cellToPaste.Text;
                    else PasteSingleCell(activeSheet, cellToPaste, cell.Model);
                }
            }
            else 
            {
                foreach (var cellToPaste in _clipboard)
                {
                    if (_copyTextOnly) PasteCopiedCellTextOnly(pasteIntoCell, cellToPaste, _centerOfCopy);
                    else PasteCopiedCell(activeSheet, pasteIntoCell, cellToPaste, _centerOfCopy);
                }
            }
            activeSheet.UpdateLayout();
        }

        private static void PasteCopiedCell(SheetViewModel activeSheet, CellViewModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy)
        {
            if (!TryGetCellToReplace(pasteIntoCell, cellToPaste, centerOfCopy, out var cellToReplace)) return;
            PasteSingleCell(activeSheet, cellToPaste, cellToReplace);
        }

        private static void PasteSingleCell(SheetViewModel activeSheet, CellModel cellToPaste, CellModel cellToReplace)
        {
            var pastedCell = CopyCellWithUpdatedLocationProperties(cellToReplace, cellToPaste);
            activeSheet.DeleteCell(cellToReplace);
            activeSheet.AddCell(pastedCell);
            var populateName = pastedCell.PopulateFunctionName;
            pastedCell.PopulateFunctionName = string.Empty;
            pastedCell.PopulateFunctionName = populateName;
        }

        private static void PasteCopiedCellTextOnly(CellViewModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy)
        {
            if (!TryGetCellToReplace(pasteIntoCell, cellToPaste, centerOfCopy, out var cellToReplace)) return;
            cellToReplace.Text = cellToPaste.Text;
        }

        private static bool TryGetCellToReplace(CellViewModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy, [MaybeNullWhen(false)] out CellModel cellToReplace)
        {
            var newRow = pasteIntoCell.Row + cellToPaste.Row - centerOfCopy.Row;
            var newColumn = pasteIntoCell.Column + cellToPaste.Column - centerOfCopy.Column;
            cellToReplace = Cells.Instance.GetCell(pasteIntoCell.Model.SheetName, newRow, newColumn);
            if (cellToReplace is null) return false;
            if (cellToReplace.CellType.IsSpecial()) return false;
            return true;
        }

        private static CellModel CopyCellWithUpdatedLocationProperties(CellModel cellToReplace, CellModel cellToPaste)
        {
            var pastedCell = cellToPaste.Copy();
            pastedCell.SheetName = cellToReplace.SheetName;
            pastedCell.Width = cellToReplace.Width;
            pastedCell.Height = cellToReplace.Height;
            pastedCell.Row = cellToReplace.Row;
            pastedCell.Column = cellToReplace.Column;
            pastedCell.MergedWith = string.Empty;
            return pastedCell;
        }
    }
}
