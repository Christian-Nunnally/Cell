using Cell.Data;
using Cell.Model;

namespace Cell.ViewModel.Application
{
    public static class UndoRedoManager
    {
        private static readonly Stack<List<CellModel>> _redoStack = new();
        private static readonly Stack<List<CellModel>> _undoStack = new();
        private static readonly Stack<CellModel> _recordingStateList = new();

        private static bool _isRecordingUndoState;
        private static bool _isUndoingOrRedoing;

        public static IEnumerable<string> UndoStack => _undoStack.Select(x => x.Count.ToString());

        public static event Action? UndoStackChanged;

        public static void StartRecordingUndoState()
        {
            if (_isRecordingUndoState) return;
            if (_isUndoingOrRedoing) return;
            _recordingStateList.Clear();
            _isRecordingUndoState = true;
        }

        public static void RecordStateIfRecording(CellModel cell)
        {
            if (_isUndoingOrRedoing) return;
            if (_isRecordingUndoState)
            {
                _recordingStateList.Push(cell.Copy());
            }
        }

        public static void FinishRecordingUndoState() 
        {
            if (!_isRecordingUndoState) return;
            if (_isUndoingOrRedoing) return;
            RecordCellStatesOntoUndoStack(_recordingStateList);
            _isRecordingUndoState = false;
        }

        public static void RecordCellStatesOntoUndoStack(IEnumerable<CellModel> cellsToRecordTheStateOf)
        {
            _undoStack.Push(cellsToRecordTheStateOf.Select(x => x.Copy()).ToList());
            _redoStack.Clear();
            UndoStackChanged?.Invoke();
        }

        public static void Redo()
        {
            _isUndoingOrRedoing = true;
            ApplyStateFromStack(_redoStack, _undoStack);
            ApplicationViewModel.Instance.SheetViewModel.UpdateLayout();
            _isUndoingOrRedoing = false;
            UndoStackChanged?.Invoke();
        }

        public static void Undo()
        {
            _isUndoingOrRedoing = true;
            ApplyStateFromStack(_undoStack, _redoStack);
            ApplicationViewModel.Instance.SheetViewModel.UpdateLayout();
            _isUndoingOrRedoing = false;
            UndoStackChanged?.Invoke();
        }

        private static void ApplyStateFromStack(Stack<List<CellModel>> stackToRestoreStateFrom, Stack<List<CellModel>> stackToSaveOldState)
        {
            var redoItems = new List<CellModel>();
            if (stackToRestoreStateFrom.Count == 0) return;
            var cellsToRestore = stackToRestoreStateFrom.Pop();
            foreach (var cellToCopyFrom in cellsToRestore)
            {
                var cellToRestoreInto = CellTracker.Instance.GetCell(cellToCopyFrom.SheetName, cellToCopyFrom.Row, cellToCopyFrom.Column);
                if (cellToRestoreInto == null) continue;
                redoItems.Add(cellToRestoreInto.Copy());
                RestoreCell(cellToRestoreInto, cellToCopyFrom);
            }
            stackToSaveOldState.Push(redoItems);
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
    }
}
