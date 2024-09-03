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
            var cellTriggerManager = new CellTriggerManager(pluginFunctionLoader);
            var cellPopulateManager = new CellPopulateManager(pluginFunctionLoader);
            var userCollectionLoader = new UserCollectionLoader(persistenceManager, cellPopulateManager);
            var cellLoader = new CellLoader(persistenceManager, sheetTracker, pluginFunctionLoader, userCollectionLoader);
            var _ = new CellTracker(sheetTracker, cellTriggerManager, cellPopulateManager, cellLoader);
        }
    }
}