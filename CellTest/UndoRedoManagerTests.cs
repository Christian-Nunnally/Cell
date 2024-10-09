using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Application;
using CellTest.TestUtilities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest
{
    public class UndoRedoManagerTests
    {
        private CellTracker _cellTracker;
        private PersistedDirectory _persistanceManager;
        private CellLoader _cellLoader;

        private UndoRedoManager GetInstance()
        {
            _persistanceManager = new PersistedDirectory("", new DictionaryFileIO());
            _cellLoader = new CellLoader(_persistanceManager);
            _cellTracker = new CellTracker(_cellLoader);
            return new UndoRedoManager(_cellTracker);
        }

        [Fact]
        public void BasicLaunchTest()
        {
            var _ = GetInstance();
        }

        [Fact]
        public void StartRecordingUndoState_Runs()
        {
            var testing = GetInstance();
            testing.StartRecordingUndoState();
        }

        [Fact]
        public void FinishRecordingUndoState_Runs()
        {
            var testing = GetInstance();
            testing.FinishRecordingUndoState();
        }

        [Fact]
        public void RecordStateIfRecording_Runs()
        {
            var testing = GetInstance();
            testing.RecordStateIfRecording(CellModel.Null);
        }

        [Fact]
        public void Undo_Runs()
        {
            var testing = GetInstance();
            testing.Undo();
        }

        [Fact]
        public void Redo_Runs()
        {
            var testing = GetInstance();
            testing.Redo();
        }

        [Fact]
        public void SingleChangeRecordedButCellNotTracked_Undo_OldStateNotRestored()
        {
            var testing = GetInstance();
            var model = new CellModel();
            testing.StartRecordingUndoState();
            testing.RecordStateIfRecording(model);
            testing.FinishRecordingUndoState();
            model.Text = "New Text";

            testing.Undo();
            
            Assert.Equal("New Text", model.Text);
        }

        [Fact]
        public void SingleChangeRecordedOnTrackedCell_Undo_OldStateRestored()
        {
            var testing = GetInstance();
            var model = new CellModel();
            _cellTracker!.AddCell(model);
            testing.StartRecordingUndoState();
            testing.RecordStateIfRecording(model);
            testing.FinishRecordingUndoState();
            var oldText = model.Text;
            model.Text = "New Text";

            testing.Undo();

            Assert.Equal(oldText, model.Text);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.