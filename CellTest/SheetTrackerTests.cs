using Cell.Data;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using Cell.View.ToolWindow;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest
{
    public class SheetTrackerTests
    {
        private CellTracker _cellTracker;
        private PluginFunctionLoader _pluginFunctionLoader;
        private CellPopulateManager _cellPopulateManager;
        private UserCollectionLoader _userCollectionLoader;
        private TestFileIO _testFileIO;
        private PersistenceManager _persistenceManager;
        private CellLoader _cellLoader;

        private SheetTracker CreateInstance()
        {
            _testFileIO = new TestFileIO();
            _persistenceManager = new PersistenceManager("", _testFileIO);
            _cellLoader = new CellLoader(_persistenceManager);
            _cellTracker = new CellTracker(_cellLoader);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistenceManager);
            _cellPopulateManager = new CellPopulateManager(_cellTracker, _pluginFunctionLoader);
            _userCollectionLoader = new UserCollectionLoader(_persistenceManager, _cellPopulateManager, _pluginFunctionLoader, _cellTracker);
            return new SheetTracker(_persistenceManager, _cellLoader, _cellTracker, _pluginFunctionLoader, _userCollectionLoader);
        }

        [Fact]
        public void BasicLaunchTest()
        {
            var _ = CreateInstance();
        }

        [Fact]
        public void NoCells_CellAddedToTracker_SheetCreatedWithNameOfCellsSheet()
        {
            var testing = CreateInstance();
            var cell = new CellModel() { SheetName = "Sheet1" };

            _cellTracker.AddCell(cell, false);

            Assert.Equal("Sheet1", testing.Sheets[0].Name);
        }

        [Fact]
        public void NoCells_CellAddedToTracker_SheetDirectoryCreatedWithNameOfCellsSheet()
        {
            var testing = CreateInstance();
            var cell = new CellModel() { SheetName = "Sheet1" };

            _cellTracker.AddCell(cell, true);

            Assert.True(_testFileIO.Exists("Sheet1"));
        }

        [Fact]
        public void SingleCell_CellRemovedFromTracker_Sheet1NoLongerExists()
        {
            var testing = CreateInstance();
            var cell = new CellModel() { SheetName = "Sheet1" };
            _cellTracker.AddCell(cell, false);
            Assert.Equal("Sheet1", testing.Sheets[0].Name);

            _cellTracker.RemoveCell(cell);

            Assert.Empty(testing.Sheets);
        }

    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.