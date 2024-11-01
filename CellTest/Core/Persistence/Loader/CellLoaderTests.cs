using Cell.Core.Data.Tracker;
using Cell.Core.Persistence;
using Cell.Core.Persistence.Loader;
using Cell.Model;
using CellTest.TestUtilities;

namespace CellTest.Core.Persistence.Loader
{
    public class CellLoaderTests
    {
        private readonly DictionaryFileIO _testFileIO;
        private readonly PersistedDirectory _persistedDirectory;
        private readonly CellTracker _cellTracker;
        private readonly CellLoader _testing;

        public CellLoaderTests()
        {
            _testFileIO = new DictionaryFileIO();
            _cellTracker = new CellTracker();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _testing = new CellLoader(_persistedDirectory, _cellTracker);
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void TrackedCellBackgroundChanged_ChangeSaved()
        {
            var cell = CellModelFactory.Create(CellType.Label, new CellLocationModel());
            _cellTracker.AddCell(cell);
            _testFileIO.DeleteAll();
            Assert.Empty(_persistedDirectory.GetFiles(""));

            cell.Style.BackgroundColor = "#ebeefe";

            Assert.NotEmpty(_persistedDirectory.GetFiles(""));
        }
    }
}
