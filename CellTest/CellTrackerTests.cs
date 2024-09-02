using Cell.Data;
using Cell.Execution;
using Cell.Persistence;

namespace CellTest
{
    public class CellTrackerTests
    {
        [Fact]
        public void BasicLaunchTest()
        {
            var testFileIO = new TestFileIO();
            var persistenceManager = new PersistenceManager("", testFileIO);
            var pluginFunctionLoader = new PluginFunctionLoader(persistenceManager);
            var sheetTracker = new SheetTracker();
            var triggerManager = new CellTriggerManager();
            var populateManager = new CellPopulateManager(pluginFunctionLoader);
            var cellLoader = new CellLoader(persistenceManager, sheetTracker, pluginFunctionLoader);
            var _ = new CellTracker(sheetTracker, triggerManager, populateManager, cellLoader);
        }
    }
}