using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.ViewModel.Application;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Cell.Persistence
{
    /// <summary>
    /// A class that handles holding on to copied cells and knows how to paste them intelligently into other cells.
    /// </summary>
    public class CellClipboard
    {
        private readonly CellTracker _cellTracker;
        private readonly ITextClipboard _textClipboard;
        private readonly UndoRedoManager _undoRedoManager;
        private bool _copyTextOnly = false;
        /// <summary>
        /// Creates a new instance of <see cref="CellClipboard"/>.
        /// </summary>
        /// <param name="undoRedoManager">The undo redo manager to record pastes onto so they can be undone.</param>
        /// <param name="cellTracker">The cell tracker to get cells from.</param>
        /// <param name="textClipboard">A text based clipboard that will be the underlying source of pasted information.</param>
        public CellClipboard(UndoRedoManager undoRedoManager, CellTracker cellTracker, ITextClipboard textClipboard)
        {
            _textClipboard = textClipboard;
            _undoRedoManager = undoRedoManager;
            _cellTracker = cellTracker;
        }

        /// <summary>
        /// Copies the given cells into the underlying clipboard.
        /// </summary>
        /// <param name="cells">The list of cells to copy.</param>
        /// <param name="copyTextOnly">Whether to copy just the text in the cells, or all settings of the cell.</param>
        public void CopyCells(IEnumerable<CellModel> cells, bool copyTextOnly)
        {
            _copyTextOnly = copyTextOnly;
            _textClipboard.Clear();
            _textClipboard.SetText(JsonSerializer.Serialize(cells));
        }

        /// <summary>
        /// Attempts to paste the copied cells into the selected cells.
        /// </summary>
        /// <param name="pasteIntoCellStart">The cell to consider as top left when pasting multiple cells.</param>
        /// <param name="selectedCells"></param>
        public void PasteIntoCells(CellModel pasteIntoCellStart, IEnumerable<CellModel> selectedCells)
        {
            // TODO: do I need pasteIntoCellStart?
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

            if (clipboard.Count() == 1) PasteTheOneCopiedCellIntoAllSelectedCells(selectedCells, clipboard);
            else PasteEachCopiedCellInRespectingOffsetFromStart(pasteIntoCellStart, clipboard, centerOfCopy);
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

        private void PasteEachCopiedCellInRespectingOffsetFromStart(CellModel pasteIntoCellStart, IEnumerable<CellModel> clipboard, CellModel centerOfCopy)
        {
            foreach (var cellToPaste in clipboard)
            {
                if (_copyTextOnly) PasteCopiedCellTextOnly(pasteIntoCellStart, cellToPaste, centerOfCopy);
                else PasteCopiedCell(pasteIntoCellStart, cellToPaste, centerOfCopy);
            }
        }

        private void PasteSingleCell(CellModel cellToPaste, CellModel cellToReplace)
        {
            _undoRedoManager.RecordStateIfRecording(cellToReplace);
            List<string> blacklist = [
                nameof(CellModel.ID),
                nameof(CellModel.Location),
                nameof(CellModel.Width),
                nameof(CellModel.Height),
                nameof(CellModel.MergedWith),
                nameof(CellModel.Value),
                nameof(CellModel.Date),
                nameof(CellModel.Int)];
            if (cellToReplace.CellType.IsSpecial()) blacklist.Add(nameof(CellModel.CellType));
            cellToPaste.CopyPublicProperties(cellToReplace, [.. blacklist]);
            cellToPaste.Style.CopyTo(cellToReplace.Style);
        }

        private void PasteTheOneCopiedCellIntoAllSelectedCells(IEnumerable<CellModel> selectedCells, IEnumerable<CellModel> clipboard)
        {
            var cellToPaste = clipboard.First();
            foreach (var cell in selectedCells.ToList())
            {
                if (_copyTextOnly) cell.Text = cellToPaste.Text;
                else PasteSingleCell(cellToPaste, cell);
            }
        }

        private bool TryGetCellToReplace(CellModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy, [MaybeNullWhen(false)] out CellModel cellToReplace)
        {
            var newRow = pasteIntoCell.Location.Row + cellToPaste.Location.Row - centerOfCopy.Location.Row;
            var newColumn = pasteIntoCell.Location.Column + cellToPaste.Location.Column - centerOfCopy.Location.Column;
            cellToReplace = _cellTracker.GetCell(pasteIntoCell.Location.SheetName, newRow, newColumn);
            if (cellToReplace is null) return false;
            if (cellToReplace.CellType.IsSpecial()) return false;
            return true;
        }
    }
}
