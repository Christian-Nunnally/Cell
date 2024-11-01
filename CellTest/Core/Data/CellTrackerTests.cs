using Cell.Core.Common;
using Cell.Core.Data;
using Cell.Core.Data.Tracker;
using Cell.Core.Execution;
using Cell.Core.Persistence;
using Cell.Model;

namespace CellTest.Core.Data
{
    public class CellTrackerTests
    {
        private readonly CellTracker _testing;

        public CellTrackerTests()
        {
            _testing = new CellTracker();
        }

        [Fact]
        public void BasicLaunchTest()
        {
        }

        [Fact]
        public void TrackedCell_CellIdChanged_ExceptionThrown()
        {
            var cell = new CellModel();
            _testing.AddCell(cell);

            Assert.Throws<CellError>(() => cell.ID = "newid");
        }
    }
}