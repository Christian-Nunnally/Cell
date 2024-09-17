using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using CellTest.TestUtilities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest
{
    public class CellPopulateManagerTests
    {
        private TestFileIO _testFileIO;
        private PersistedDirectory _persistenceManager;
        private CellLoader _cellLoader;
        private CellTracker _cellTracker;
        private PluginFunctionLoader _pluginFunctionLoader;
        private UserCollectionLoader _userCollectionLoader;

        private CellPopulateManager CreateInstance()
        {
            _testFileIO = new TestFileIO();
            _persistenceManager = new PersistedDirectory("", _testFileIO);
            _cellLoader = new CellLoader(_persistenceManager);
            _cellTracker = new CellTracker(_cellLoader);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistenceManager);
            _userCollectionLoader = new UserCollectionLoader(_persistenceManager, _pluginFunctionLoader, _cellTracker);
            return new CellPopulateManager(_cellTracker, _pluginFunctionLoader, _userCollectionLoader);
        }

        [Fact]
        public void BasicLaunchTest()
        {
            var _ = CreateInstance();
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.