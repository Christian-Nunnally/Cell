using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Application;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest
{
    public class UndoRedoManagerTests
    {
        private CellTracker _cellTracker;
        private PersistenceManager _persistanceManager;
        private PluginFunctionLoader _pluginFunctionLoader;
        private CellPopulateManager _populateManager;
        private UserCollectionLoader _userCollectionLoader;
        private CellLoader _cellLoader;
        private CellTriggerManager _triggerManager;

        private UndoRedoManager GetInstance()
        {
            _persistanceManager = new PersistenceManager("", new TestFileIO());
            _pluginFunctionLoader = new PluginFunctionLoader(_persistanceManager);
            _cellLoader = new CellLoader(_persistanceManager);
            _cellTracker = new CellTracker(_cellLoader);
            _populateManager = new CellPopulateManager(_cellTracker, _pluginFunctionLoader);
            _userCollectionLoader = new UserCollectionLoader(_persistanceManager, _populateManager, _pluginFunctionLoader, _cellTracker);
            _triggerManager = new CellTriggerManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
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