using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest
{
    public class CellPopulateManagerTests
    {
        private TestFileIO _testFileIO;
        private PersistenceManager _persistenceManager;
        private CellLoader _cellLoader;
        private CellTracker _cellTracker;
        private PluginFunctionLoader _pluginFunctionLoader;

        private CellPopulateManager CreateInstance()
        {
            _testFileIO = new TestFileIO();
            _persistenceManager = new PersistenceManager("", _testFileIO);
            _cellLoader = new CellLoader(_persistenceManager);
            _cellTracker = new CellTracker(_cellLoader);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistenceManager);
            return new CellPopulateManager(_cellTracker, _pluginFunctionLoader);
        }

        [Fact]
        public void BasicLaunchTest()
        {
            var _ = CreateInstance();
        }

        [Fact]
        public void CellSubscribedToCollectionUpdates_NotifyCollectionUpdated_CellPopulateFunctionRun()
        {
            var testing = CreateInstance();
            var cellModel = new CellModel();
            var testCollectionName = "testCollection";
            _pluginFunctionLoader!.CreateFunction("object", "testFunction", "return 1;");
            cellModel.PopulateFunctionName = "testFunction";
            testing.SubscribeToCollectionUpdates(cellModel, testCollectionName);
            Assert.NotEqual("1", cellModel.Text);

            testing.NotifyCollectionUpdated(testCollectionName);

            Assert.Equal("1", cellModel.Text);
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.