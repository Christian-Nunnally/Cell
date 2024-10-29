using Cell.Core.Data;
using Cell.Model;
using Cell.Core.Persistence;
using CellTest.TestUtilities;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest.Core.Data
{
    public class SheetTrackerTests
    {
        private CellTracker _cellTracker;
        private DictionaryFileIO _testFileIO;
        private PersistedDirectory _persistedDirectory;

        private SheetTracker CreateInstance()
        {
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellTracker = new CellTracker();
            var _ = new CellLoader(_persistedDirectory, _cellTracker);
            return new SheetTracker(_cellTracker);
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

            _cellTracker.AddCell(cell);

            Assert.Equal("Sheet1", testing.Sheets[0].Name);
        }

        [Fact]
        public void NoCells_CellAddedToTracker_SheetDirectoryCreatedWithNameOfCellsSheet()
        {
            var _ = CreateInstance();
            var cell = new CellModel();
            cell.Location.SheetName = "Sheet1";

            _cellTracker.AddCell(cell);

            Assert.True(_testFileIO.DirectoryExists(Path.Combine("Sheet1")));
        }

        [Fact]
        public void SingleCell_CellRemovedFromTracker_Sheet1NoLongerExists()
        {
            var testing = CreateInstance();
            var cell = new CellModel();
            cell.Location.SheetName = "Sheet1";
            _cellTracker.AddCell(cell);
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
            _cellTracker.AddCell(cell);
            Assert.True(_testFileIO.DirectoryExists(Path.Combine("Sheet1")));

            _cellTracker.RemoveCell(cell);

            Assert.False(_testFileIO.DirectoryExists(Path.Combine("Sheet1")));
        }

    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.