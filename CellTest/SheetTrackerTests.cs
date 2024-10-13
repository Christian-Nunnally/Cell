using Cell.Core.Data;
using Cell.Model;
using Cell.Core.Persistence;
using CellTest.TestUtilities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest
{
    public class SheetTrackerTests
    {
        private CellTracker _cellTracker;
        private PluginFunctionLoader _pluginFunctionLoader;
        private UserCollectionLoader _userCollectionLoader;
        private DictionaryFileIO _testFileIO;
        private PersistedDirectory _persistedDirectory;
        private CellLoader _cellLoader;

        private SheetTracker CreateInstance()
        {
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellLoader = new CellLoader(_persistedDirectory);
            _cellTracker = new CellTracker(_cellLoader);
            _pluginFunctionLoader = new PluginFunctionLoader(_persistedDirectory);
            _userCollectionLoader = new UserCollectionLoader(_persistedDirectory, _pluginFunctionLoader, _cellTracker);
            return new SheetTracker(_persistedDirectory, _cellLoader, _cellTracker, _pluginFunctionLoader, _userCollectionLoader);
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
            var cell = new CellModel();
            cell.Location.SheetName = "Sheet1";

            _cellTracker.AddCell(cell, false);

            Assert.Equal("Sheet1", testing.Sheets[0].Name);
        }

        [Fact]
        public void NoCells_CellAddedToTracker_SheetDirectoryCreatedWithNameOfCellsSheet()
        {
            var _ = CreateInstance();
            var cell = new CellModel();
            cell.Location.SheetName = "Sheet1";

            _cellTracker.AddCell(cell, true);

            Assert.True(_testFileIO.DirectoryExists(Path.Combine("Sheets", "Sheet1")));
        }

        [Fact]
        public void SingleCell_CellRemovedFromTracker_Sheet1NoLongerExists()
        {
            var testing = CreateInstance();
            var cell = new CellModel();
            cell.Location.SheetName = "Sheet1";
            _cellTracker.AddCell(cell, false);
            Assert.Equal("Sheet1", testing.Sheets[0].Name);

            _cellTracker.RemoveCell(cell);

            Assert.Empty(testing.Sheets);
        }

        [Fact]
        public void SingleCellSaved_CellRemovedFromTracker_Sheet1DirectoryNoLongerExists()
        {
            var _ = CreateInstance();
            var cell = new CellModel();
            cell.Location.SheetName = "Sheet1";
            _cellTracker.AddCell(cell, true);
            Assert.True(_testFileIO.DirectoryExists(Path.Combine("Sheets", "Sheet1")));

            _cellTracker.RemoveCell(cell);

            Assert.False(_testFileIO.DirectoryExists(Path.Combine("Sheets", "Sheet1")));
        }

    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.