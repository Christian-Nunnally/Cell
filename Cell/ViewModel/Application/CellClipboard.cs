using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.ViewModel.Cells;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Windows;

namespace Cell.ViewModel.Application
{
    public class CellClipboard
    {
        private readonly CellTracker _cellTracker;
        private readonly UndoRedoManager _undoRedoManager;
        private CellModel? _centerOfCopy;
        private IEnumerable<CellModel> _clipboard = [];
        private bool _copyTextOnly = false;
        public CellClipboard(UndoRedoManager undoRedoManager, CellTracker cellTracker)
        {
            _undoRedoManager = undoRedoManager;
            _cellTracker = cellTracker;
        }

        public void CopySelectedCells(SheetViewModel activeSheet, bool copyTextOnly)
        {
            _copyTextOnly = copyTextOnly;
            _clipboard = activeSheet.SelectedCellViewModels.Select(c => c.Model).ToList();
            Clipboard.Clear();
            Clipboard.SetText(JsonSerializer.Serialize(_clipboard));
        }

        public void PasteIntoCells(CellViewModel pasteIntoCellStart, IEnumerable<CellViewModel> selectedCellViewModels)
        {
            if (Clipboard.ContainsText())
            {
                try
                {
                    if (JsonSerializer.Deserialize(Clipboard.GetText(), typeof(List<CellModel>)) is List<CellModel> cellsFromClipboard)
                    {
                        _centerOfCopy = cellsFromClipboard.FirstOrDefault();
                        _clipboard = cellsFromClipboard;
                    }
                }
                catch
                {
                    _copyTextOnly = true;
                    _clipboard = [new CellModel { Text = Clipboard.GetText() }];
                }
            }

            if (_clipboard is null) return;
            if (_centerOfCopy is null) return;
            if (!_clipboard.Any()) return;

            if (_clipboard.Count() == 1)
            {
                var cellToPaste = _clipboard.First();
                foreach (var cell in selectedCellViewModels.ToList())
                {
                    if (_copyTextOnly) cell.Text = cellToPaste.Text;
                    else PasteSingleCell(cellToPaste, cell.Model);
                }
            }
            else
            {
                foreach (var cellToPaste in _clipboard)
                {
                    if (_copyTextOnly) PasteCopiedCellTextOnly(pasteIntoCellStart, cellToPaste, _centerOfCopy);
                    else PasteCopiedCell(pasteIntoCellStart, cellToPaste, _centerOfCopy);
                }
            }
        }

        private void PasteCopiedCell(CellViewModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy)
        {
            if (!TryGetCellToReplace(pasteIntoCell, cellToPaste, centerOfCopy, out var cellToReplace)) return;
            PasteSingleCell(cellToPaste, cellToReplace);
        }

        private void PasteCopiedCellTextOnly(CellViewModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy)
        {
            if (!TryGetCellToReplace(pasteIntoCell, cellToPaste, centerOfCopy, out var cellToReplace)) return;
            _undoRedoManager.RecordStateIfRecording(cellToReplace);
            cellToReplace.Text = cellToPaste.Text;
        }

        private void PasteSingleCell(CellModel cellToPaste, CellModel cellToReplace)
        {
            _undoRedoManager.RecordStateIfRecording(cellToReplace);
            cellToPaste.CopyPublicProperties(cellToReplace, [nameof(CellModel.ID), nameof(CellModel.SheetName), nameof(CellModel.Width), nameof(CellModel.Height), nameof(CellModel.Row), nameof(CellModel.Column), nameof(CellModel.MergedWith), nameof(CellModel.Value), nameof(CellModel.Date)]);
        }

        private bool TryGetCellToReplace(CellViewModel pasteIntoCell, CellModel cellToPaste, CellModel centerOfCopy, [MaybeNullWhen(false)] out CellModel cellToReplace)
        {
            var newRow = pasteIntoCell.Row + cellToPaste.Row - centerOfCopy.Row;
            var newColumn = pasteIntoCell.Column + cellToPaste.Column - centerOfCopy.Column;
            cellToReplace = _cellTracker.GetCell(pasteIntoCell.Model.SheetName, newRow, newColumn);
            if (cellToReplace is null) return false;
            if (cellToReplace.CellType.IsSpecial()) return false;
            return true;
        }
    }
}
