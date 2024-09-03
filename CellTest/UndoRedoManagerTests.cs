using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Application;

namespace CellTest
{
    public class UndoRedoManagerTests
    {
        private CellTracker? _cellTracker;

        private UndoRedoManager GetInstance()
        {
            var persistanceManager = new PersistenceManager("", new TestFileIO());
            var pluginFunctionLoader = new PluginFunctionLoader(persistanceManager);
            var sheetTracker = new SheetTracker();
            var populateManager = new CellPopulateManager(pluginFunctionLoader);
            var userCollectionLoader = new UserCollectionLoader(persistanceManager, populateManager);
            var cellLoader = new CellLoader(persistanceManager, sheetTracker, pluginFunctionLoader, userCollectionLoader);
            var triggerManager = new CellTriggerManager(pluginFunctionLoader);
            _cellTracker = new CellTracker(sheetTracker, triggerManager, populateManager, cellLoader);
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
            testing.RecordStateIfRecording(CellModel.Empty);
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
