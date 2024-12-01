using Cell.Core.Common;
using Cell.Core.Data.Tracker;
using Cell.Model;
using Cell.ViewModel.Application;

namespace CellTest.Core.Persistence
{
    public class UndoRedoManagerTests
    {
        private readonly CellTracker _cellTracker;
        private readonly UndoRedoManager _testing;
        private readonly FunctionTracker _functionTracker;
        private readonly Logger _logger;

        public UndoRedoManagerTests()
        {
            _cellTracker = new CellTracker();
            _logger = new Logger();
            _functionTracker = new FunctionTracker(_logger);
            _testing = new UndoRedoManager(_cellTracker, _functionTracker);
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void StartRecordingUndoState_Runs()
        {
            _testing.StartRecordingUndoState();
        }

        [Fact]
        public void FinishRecordingUndoState_Runs()
        {
            _testing.FinishRecordingUndoState();
        }

        [Fact]
        public void RecordStateIfRecording_Runs()
        {
            _testing.RecordStateIfRecording(CellModel.Null);
        }

        [Fact]
        public void Undo_Runs()
        {
            _testing.Undo();
        }

        [Fact]
        public void Redo_Runs()
        {
            _testing.Redo();
        }

        [Fact]
        public void SingleChangeRecordedButCellNotTracked_Undo_OldStateNotRestored()
        {
            var model = new CellModel();
            _testing.StartRecordingUndoState();
            _testing.RecordStateIfRecording(model);
            _testing.FinishRecordingUndoState();
            model.Text = "New Text";

            _testing.Undo();

            Assert.Equal("New Text", model.Text);
        }

        [Fact]
        public void SingleChangeRecordedOnTrackedCell_Undo_OldStateRestored()
        {
            var model = new CellModel();
            _cellTracker!.AddCell(model);
            _testing.StartRecordingUndoState();
            _testing.RecordStateIfRecording(model);
            _testing.FinishRecordingUndoState();
            var oldText = model.Text;
            model.Text = "New Text";

            _testing.Undo();

            Assert.Equal(oldText, model.Text);
        }
    }
}
