using Cell.Data;
using Cell.Model;

namespace Cell.ViewModel.Application
{
    public class UndoRedoManager
    {
        private readonly CellTracker _cellTracker;
        private readonly List<string> _recordingStateIdList = [];
        private readonly Stack<CellModel> _recordingStateList = new();
        private readonly Stack<List<CellModel>> _redoStack = new();
        private readonly Stack<List<CellModel>> _undoStack = new();
        private bool _isRecordingUndoState;
        private bool _isUndoingOrRedoing;
        public UndoRedoManager(CellTracker cellTracker)
        {
            _cellTracker = cellTracker;
        }

        public event Action? UndoStackChanged;

        public IEnumerable<string> UndoStack => _undoStack.Select(x => x.Count.ToString());

        public void FinishRecordingUndoState()
        {
            if (!_isRecordingUndoState) return;
            if (_isUndoingOrRedoing) return;
            RecordCellStatesOntoUndoStack(_recordingStateList);
            _isRecordingUndoState = false;
        }

        public void RecordStateIfRecording(CellModel cell)
        {
            if (_isUndoingOrRedoing) return;
            if (_isRecordingUndoState)
            {
                if (_recordingStateIdList.Contains(cell.ID)) return;
                _recordingStateList.Push(cell.Copy());
                _recordingStateIdList.Add(cell.ID);
            }
        }

        public void RecordStateIfRecording(IEnumerable<CellModel> cells)
        {
            foreach (var cell in cells) RecordStateIfRecording(cell);
        }

        public void Redo()
        {
            _isUndoingOrRedoing = true;
            ApplyStateFromStack(_redoStack, _undoStack);
            // TODO: Make this not required.
            ApplicationViewModel.SafeInstance?.SheetViewModel?.UpdateLayout();
            _isUndoingOrRedoing = false;
            UndoStackChanged?.Invoke();
        }

        public void StartRecordingUndoState()
        {
            if (_isRecordingUndoState) return;
            if (_isUndoingOrRedoing) return;
            _recordingStateList.Clear();
            _recordingStateIdList.Clear();
            _isRecordingUndoState = true;
        }

        public void Undo()
        {
            _isUndoingOrRedoing = true;
            ApplyStateFromStack(_undoStack, _redoStack);
            // TODO: Make this not required.
            ApplicationViewModel.SafeInstance?.SheetViewModel?.UpdateLayout();
            _isUndoingOrRedoing = false;
            UndoStackChanged?.Invoke();
        }

        private static void RestoreCell(CellModel cellToRestoreInto, CellModel cellToCopyFrom)
        {
            cellToRestoreInto.Width = cellToCopyFrom.Width;
            cellToRestoreInto.Height = cellToCopyFrom.Height;
            cellToRestoreInto.CellType = cellToCopyFrom.CellType;
            cellToRestoreInto.Column = cellToCopyFrom.Column;
            cellToRestoreInto.Row = cellToCopyFrom.Row;
            cellToRestoreInto.SheetName = cellToCopyFrom.SheetName;
            cellToRestoreInto.MergedWith = cellToCopyFrom.MergedWith;
            cellToRestoreInto.Text = cellToCopyFrom.Text;
            cellToRestoreInto.Index = cellToCopyFrom.Index;
            cellToRestoreInto.ColorHexes = cellToCopyFrom.ColorHexes;
            cellToRestoreInto.SetBackground(cellToCopyFrom.ColorHexes[(int)ColorFor.Background]);
            cellToRestoreInto.SetForeground(cellToCopyFrom.ColorHexes[(int)ColorFor.Foreground]);
            cellToRestoreInto.SetContentBackground(cellToCopyFrom.ColorHexes[(int)ColorFor.ContentBackground]);
            cellToRestoreInto.SetContentBorder(cellToCopyFrom.ColorHexes[(int)ColorFor.ContentBorder]);
            cellToRestoreInto.SetContentHighlight(cellToCopyFrom.ColorHexes[(int)ColorFor.ContentHighlight]);
            cellToRestoreInto.SetBorder(cellToCopyFrom.ColorHexes[(int)ColorFor.Border]);
            cellToRestoreInto.BorderThicknessString = cellToCopyFrom.BorderThicknessString;
            cellToRestoreInto.ContentBorderThicknessString = cellToCopyFrom.ContentBorderThicknessString;
            cellToRestoreInto.MarginString = cellToCopyFrom.MarginString;
            cellToRestoreInto.FontSize = cellToCopyFrom.FontSize;
            cellToRestoreInto.FontFamily = cellToCopyFrom.FontFamily;
            cellToRestoreInto.IsFontBold = cellToCopyFrom.IsFontBold;
            cellToRestoreInto.IsFontItalic = cellToCopyFrom.IsFontItalic;
            cellToRestoreInto.IsFontStrikethrough = cellToCopyFrom.IsFontStrikethrough;
            cellToRestoreInto.HorizontalAlignment = cellToCopyFrom.HorizontalAlignment;
            cellToRestoreInto.VerticalAlignment = cellToCopyFrom.VerticalAlignment;
            cellToRestoreInto.TextAlignmentForView = cellToCopyFrom.TextAlignmentForView;
            cellToRestoreInto.PopulateFunctionName = cellToCopyFrom.PopulateFunctionName;
            cellToRestoreInto.TriggerFunctionName = cellToCopyFrom.TriggerFunctionName;
            cellToRestoreInto.StringProperties = cellToCopyFrom.StringProperties;
            cellToRestoreInto.BooleanProperties = cellToCopyFrom.BooleanProperties;
            cellToRestoreInto.NumericProperties = cellToCopyFrom.NumericProperties;
        }

        private void ApplyStateFromStack(Stack<List<CellModel>> stackToRestoreStateFrom, Stack<List<CellModel>> stackToSaveOldState)
        {
            var redoItems = new List<CellModel>();
            if (stackToRestoreStateFrom.Count == 0) return;
            var cellsToRestore = stackToRestoreStateFrom.Pop();
            foreach (var cellToCopyFrom in cellsToRestore)
            {
                var cellToRestoreInto = _cellTracker.GetCell(cellToCopyFrom.SheetName, cellToCopyFrom.Row, cellToCopyFrom.Column);
                if (cellToRestoreInto == null) continue;
                redoItems.Add(cellToRestoreInto.Copy());
                RestoreCell(cellToRestoreInto, cellToCopyFrom);
            }
            stackToSaveOldState.Push(redoItems);
        }

        private void RecordCellStatesOntoUndoStack(IEnumerable<CellModel> cellsToRecordTheStateOf)
        {
            _undoStack.Push(cellsToRecordTheStateOf.Select(x => x.Copy()).ToList());
            _redoStack.Clear();
            UndoStackChanged?.Invoke();
        }
    }
}
