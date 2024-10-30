using Cell.Core.Data.Tracker;
using Cell.Model;

namespace Cell.ViewModel.Application
{
    /// <summary>
    /// Manages the undo and redo stacks for the application.
    /// </summary>
    public class UndoRedoManager
    {
        private readonly CellTracker _cellTracker;
        private readonly List<string> _recordingStateIdList = [];
        private readonly Stack<CellModel> _recordingStateList = new();
        private readonly Stack<List<CellModel>> _redoStack = new();
        private readonly Stack<List<CellModel>> _undoStack = new();
        private bool _isRecordingUndoState;
        private bool _isUndoingOrRedoing;
        /// <summary>
        /// Creates a new instance of <see cref="UndoRedoManager"/> with its own stacks.
        /// </summary>
        /// <param name="cellTracker">The cell tracker used to get current cells during undo/redo.</param>
        public UndoRedoManager(CellTracker cellTracker)
        {
            _cellTracker = cellTracker;
        }

        /// <summary>
        /// Occurs when the undo stack changes.
        /// </summary>
        public event Action? UndoStackChanged;

        /// <summary>
        /// A list of string representations of the items in the redo stack.
        /// </summary>
        public IEnumerable<string> UndoStack => _undoStack.Select(x => x.Count.ToString());

        /// <summary>
        /// Completes a single atomic undo/redo operation started by <see cref="StartRecordingUndoState"/>. This will add all cells recorded by <see cref="RecordStateIfRecording(CellModel)"/> since the last call to <see cref="StartRecordingUndoState"/> to the undo stack.
        /// </summary>
        public void FinishRecordingUndoState()
        {
            if (!_isRecordingUndoState) return;
            if (_isUndoingOrRedoing) return;
            RecordCellStatesOntoUndoStack(_recordingStateList);
            _isRecordingUndoState = false;
        }

        /// <summary>
        /// If <see cref="StartRecordingUndoState"/> has been called, adds the state of the given cell to the group of states that will be a single undo operation next time <see cref="FinishRecordingUndoState"/> is called.
        /// </summary>
        /// <param name="cell"></param>
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

        /// <summary>
        /// Pop the last redo state and apply it after adding the current state to the undo stack.
        /// </summary>
        public void Redo()
        {
            _isUndoingOrRedoing = true;
            ApplyStateFromStack(_redoStack, _undoStack);
            // TODO: Make this not required.
            ApplicationViewModel.SafeInstance?.SheetViewModel?.UpdateLayout();
            _isUndoingOrRedoing = false;
            UndoStackChanged?.Invoke();
        }

        /// <summary>
        /// Begins recording a new undo state. This will group all states given to <see cref="RecordStateIfRecording(CellModel)"/> until <see cref="FinishRecordingUndoState"/> is called into a single undo operation.
        /// </summary>
        public void StartRecordingUndoState()
        {
            if (_isRecordingUndoState) return;
            if (_isUndoingOrRedoing) return;
            _recordingStateList.Clear();
            _recordingStateIdList.Clear();
            _isRecordingUndoState = true;
        }

        /// <summary>
        /// Pop the last undo state and apply it after adding the current state to the redo stack.
        /// </summary>
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
            cellToRestoreInto.MergedWith = cellToCopyFrom.MergedWith;
            cellToRestoreInto.Text = cellToCopyFrom.Text;
            cellToRestoreInto.Index = cellToCopyFrom.Index;
            cellToRestoreInto.PopulateFunctionName = cellToCopyFrom.PopulateFunctionName;
            cellToRestoreInto.TriggerFunctionName = cellToCopyFrom.TriggerFunctionName;
            cellToRestoreInto.Properties = cellToCopyFrom.Properties;
            cellToCopyFrom.Style.CopyTo(cellToRestoreInto.Style);
            cellToCopyFrom.Location.CopyTo(cellToRestoreInto.Location);
        }

        private void ApplyStateFromStack(Stack<List<CellModel>> stackToRestoreStateFrom, Stack<List<CellModel>> stackToSaveOldState)
        {
            var redoItems = new List<CellModel>();
            if (stackToRestoreStateFrom.Count == 0) return;
            var cellsToRestore = stackToRestoreStateFrom.Pop();
            foreach (var cellToCopyFrom in cellsToRestore)
            {
                var cellToRestoreInto = _cellTracker.GetCell(cellToCopyFrom.Location);
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
