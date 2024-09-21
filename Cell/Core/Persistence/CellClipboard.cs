using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Cell.Persistence
{
    public class CellClipboard
    {
        private readonly CellTracker _cellTracker;
        private readonly ITextClipboard _textClipboard;
        private readonly UndoRedoManager _undoRedoManager;
        private bool _copyTextOnly = false;
        public CellClipboard(UndoRedoManager undoRedoManager, CellTracker cellTracker, ITextClipboard textClipboard)
        {
            _textClipboard = textClipboard;
            _undoRedoManager = undoRedoManager;
            _cellTracker = cellTracker;
        }

        public void CopyCells(IEnumerable<CellModel> cells, bool copyTextOnly)
        {
            _copyTextOnly = copyTextOnly;
            _textClipboard.Clear();
            _textClipboard.SetText(JsonSerializer.Serialize(cells));
        }

        public void PasteIntoCells(CellModel pasteIntoCellStart, IEnumerable<CellModel> selectedCells)
        {
            IEnumerable<CellModel>? clipboard = null;
            if (_textClipboard.ContainsText())
            {
                try
                {
                    if (JsonSerializer.Deserialize(_textClipboard.GetText(), typeof(List<CellModel>)) is List<CellModel> cellsFromClipboard)
                    {
                        clipboard = cellsFromClipboard;
                    }
                }
                catch
                {
                    _copyTextOnly = true;
                    clipboard = [new CellModel { Text = _textClipboard.GetText() }];
                }
            }
            if (clipboard is null) return;
            if (!clipboard.Any()) return;
            var centerOfCopy = clipboard.First();

            if (clipboard.Count() == 1)
            {
                var cellToPaste = clipboard.First();
                foreach (var cell in selectedCells.ToList())
                {
                    if (_copyTextOnly) cell.Text = cellToPaste.Text;
                    else PasteSingleCell(cellToPaste, cell);
                }
            }
            else
            {
                foreach (var cellToPaste in clipboard)
                {
                    if (_copyTextOnly) PasteCopiedCellTextOnly(pasteIntoCellStart, cellToPaste, centerOfCopy);
                    else PasteCopiedCell(pasteIntoCellStart, cellToPaste, centerOfCopy);
                }
            }
        }

        private void PasteCopiedCell(CellModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy)
        {
            if (!TryGetCellToReplace(pasteIntoCell, cellToPaste, centerOfCopy, out var cellToReplace)) return;
            PasteSingleCell(cellToPaste, cellToReplace);
        }

        private void PasteCopiedCellTextOnly(CellModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy)
        {
            if (!TryGetCellToReplace(pasteIntoCell, cellToPaste, centerOfCopy, out var cellToReplace)) return;
            _undoRedoManager.RecordStateIfRecording(cellToReplace);
            cellToReplace.Text = cellToPaste.Text;
        }

        private void PasteSingleCell(CellModel cellToPaste, CellModel cellToReplace)
        {
            _undoRedoManager.RecordStateIfRecording(cellToReplace);
            List<string> blacklist = [
                nameof(CellModel.ID),
                nameof(CellModel.SheetName),
                nameof(CellModel.Width),
                nameof(CellModel.Height),
                nameof(CellModel.Row),
                nameof(CellModel.Column),
                nameof(CellModel.MergedWith),
                nameof(CellModel.Value),
                nameof(CellModel.Date),
                nameof(CellModel.Int)];
            if (cellToReplace.CellType.IsSpecial()) blacklist.Add(nameof(CellModel.CellType));
            cellToPaste.CopyPublicProperties(cellToReplace, [.. blacklist]);
            cellToPaste.Style.CopyTo(cellToReplace.Style);
        }

        private bool TryGetCellToReplace(CellModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy, [MaybeNullWhen(false)] out CellModel cellToReplace)
        {
            var newRow = pasteIntoCell.Row + cellToPaste.Row - centerOfCopy.Row;
            var newColumn = pasteIntoCell.Column + cellToPaste.Column - centerOfCopy.Column;
            cellToReplace = _cellTracker.GetCell(pasteIntoCell.SheetName, newRow, newColumn);
            if (cellToReplace is null) return false;
            if (cellToReplace.CellType.IsSpecial()) return false;
            return true;
        }
    }
}
