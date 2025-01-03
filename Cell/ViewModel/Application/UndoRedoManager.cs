﻿using Cell.Core.Common;
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
        private readonly FunctionTracker _functionTracker;
        private UndoRedoState _recordingState = new();
        private readonly Stack<UndoRedoState> _redoStack = new();
        private readonly Stack<UndoRedoState> _undoStack = new();
        private bool _isRecordingUndoState;
        private bool _isUndoingOrRedoing;
        /// <summary>
        /// Creates a new instance of <see cref="UndoRedoManager"/> with its own stacks.
        /// </summary>
        /// <param name="cellTracker">The cell tracker used to get current cells during undo/redo.</param>
        public UndoRedoManager(CellTracker cellTracker, FunctionTracker functionTracker)
        {
            _cellTracker = cellTracker;
            _functionTracker = functionTracker;
        }

        /// <summary>
        /// Occurs when the undo stack changes.
        /// </summary>
        public event Action? UndoStackChanged;

        /// <summary>
        /// A list of string representations of the items in the redo stack.
        /// </summary>
        public IEnumerable<string> UndoStack => _undoStack.Select(x => x.ToString());

        /// <summary>
        /// Completes a single atomic undo/redo operation started by <see cref="StartRecordingUndoState"/>. This will add all cells recorded by <see cref="RecordStateIfRecording(CellModel)"/> since the last call to <see cref="StartRecordingUndoState"/> to the undo stack.
        /// </summary>
        public void FinishRecordingUndoState()
        {
            if (!_isRecordingUndoState) return;
            if (_isUndoingOrRedoing) return;
            RecordStateOntoUndoStack(_recordingState);
            _cellTracker.CellRemoved -= CellRemovedWhileRecordingUndoRedo;
            _cellTracker.CellAdded -= CellAddedWhileRecordingUndoRedo;
            _isRecordingUndoState = false;
        }

        /// <summary>
        /// If <see cref="StartRecordingUndoState"/> has been called, adds the state of the given cell to the group of states that will be a single undo operation next time <see cref="FinishRecordingUndoState"/> is called.
        /// </summary>
        /// <param name="cell">Cell to record the state of.</param>
        public void RecordStateIfRecording(CellModel cell)
        {
            if (_isUndoingOrRedoing) return;
            if (_isRecordingUndoState)
            {
                if (_recordingState.CellsToRestore.Any(c => c.ID == cell.ID)) return;
                _recordingState.CellsToRestore.Add(cell.Copy());
            }
        }

        /// <summary>
        /// If <see cref="StartRecordingUndoState"/> has been called, adds the state of the given cell to the group of states that will be a single undo operation next time <see cref="FinishRecordingUndoState"/> is called.
        /// </summary>
        /// <param name="function">Function to record the state of.</param>
        public void RecordStateIfRecording(CellFunctionModel function)
        {
            if (_isUndoingOrRedoing) return;
            if (_isRecordingUndoState)
            {
                if (_recordingState.FunctionsToRestore.Any(c => c.Name == function.Name)) return;
                _recordingState.FunctionsToRestore.Add(function.Copy());
            }
        }

        /// <summary>
        /// Pop the last redo state and apply it after adding the current state to the undo stack.
        /// </summary>
        public void Redo()
        {
            _isUndoingOrRedoing = true;
            ApplyStateFromStack(_redoStack, _undoStack);
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
            _recordingState = new UndoRedoState();
            _cellTracker.CellRemoved += CellRemovedWhileRecordingUndoRedo;
            _cellTracker.CellAdded += CellAddedWhileRecordingUndoRedo;
            _isRecordingUndoState = true;
        }

        private void CellAddedWhileRecordingUndoRedo(CellModel model)
        {
            _recordingState.CellsToRemove.Add(model);
        }

        private void CellRemovedWhileRecordingUndoRedo(CellModel model)
        {
            var cellToRecord = _recordingState.CellsToRestore.FirstOrDefault(x => x.ID == model.ID, model);
            _recordingState.CellsToAdd.Add(cellToRecord);
        }

        /// <summary>
        /// Pop the last undo state and apply it after adding the current state to the redo stack.
        /// </summary>
        public void Undo()
        {
            _isUndoingOrRedoing = true;
            ApplyStateFromStack(_undoStack, _redoStack);
            _isUndoingOrRedoing = false;
            UndoStackChanged?.Invoke();
        }

        public IDisposable StartOfUndoRecording() => new UndoStateFinisher(this);

        public static IDisposable? RecordUndo() => ApplicationViewModel.Instance?.UndoRedoManager?.StartOfUndoRecording();

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

        private void ApplyStateFromStack(Stack<UndoRedoState> stackToRestoreStateFrom, Stack<UndoRedoState> stackToSaveOldState)
        {
            var redoItems = new List<CellModel>();
            if (stackToRestoreStateFrom.Count == 0) return;
            var state = stackToRestoreStateFrom.Pop();
            var cellsToRestore = state.CellsToRestore;
            foreach (var cellToCopyFrom in cellsToRestore)
            {
                var cellToRestoreInto = _cellTracker.GetCell(cellToCopyFrom.ID);
                if (cellToRestoreInto is null) continue;// throw new CellError("Undo or redo is expecting a cell to exist that does not.");
                redoItems.Add(cellToRestoreInto.Copy());
                RestoreCell(cellToRestoreInto, cellToCopyFrom);
            }
            foreach (var cellToRemove in state.CellsToRemove)
            {
                _cellTracker.RemoveCell(cellToRemove);
            }
            foreach (var cellToAdd in state.CellsToAdd)
            {
                _cellTracker.AddCell(cellToAdd);
            }
            foreach (var functionToRestore in state.FunctionsToRestore)
            {
                // TODO: maybe make function have IDs?
                //var functionToRestoreInto = _functionTracker. .GetFunction(functionToRestore.Name);
                //if (functionToRestoreInto is null) continue;// throw new CellError("Undo or redo is expecting a function to exist that does not.");
                //functionToRestore.CopyTo(functionToRestoreInto);
            }

            var redoState = new UndoRedoState { CellsToRestore = redoItems, CellsToAdd = state.CellsToRemove, CellsToRemove = state.CellsToAdd };
            stackToSaveOldState.Push(redoState);
        }

        private void RecordStateOntoUndoStack(UndoRedoState state)
        {
            _undoStack.Push(state);
            _redoStack.Clear();
            UndoStackChanged?.Invoke();
        }
    }

    public class UndoStateFinisher : IDisposable
    {
        private readonly UndoRedoManager _manager;

        public UndoStateFinisher(UndoRedoManager manager)
        {
            _manager = manager;
            _manager.StartRecordingUndoState();
        }

        void IDisposable.Dispose()
        {
            _manager.FinishRecordingUndoState();
        }
    }
}
