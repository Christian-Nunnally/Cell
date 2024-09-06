using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using Cell.View.ToolWindow;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest
{
    public class CellFormatEditWindowTests
    {
        private CellTracker _cellTracker;
        private TestFileIO _testFileIO;
        private PersistenceManager _persistenceManager;
        private CellLoader _cellLoader;

        private void CreateInstances()
        {
            _testFileIO = new TestFileIO();
            _persistenceManager = new PersistenceManager("", _testFileIO);
            _cellLoader = new CellLoader(_persistenceManager);
            _cellTracker = new CellTracker(_cellLoader);
        }

        [Fact]
        public void EmptyListOfCells_MergeCellsDown_Runs()
        {
            CreateInstances();
            var cells = new List<CellModel>();

            CellFormatEditWindow.MergeCellsDown(cells, _cellTracker);
        }

        [Fact]
        public void SingleCell_MergeCellsDown_DoesNoSetMergedWithIdToSelf()
        {
            CreateInstances();
            var cells = new List<CellModel>();
            var cell = new CellModel();
            cells.Add(cell);

            CellFormatEditWindow.MergeCellsDown(cells, _cellTracker);

            Assert.False(cell.IsMerged());
        }

        [Fact]
        public void TwoCellsInSameColumn_MergeCellsDown_BothCellsMerged()
        {
            CreateInstances();
            var cells = new List<CellModel>();
            var cell = new CellModel() { Row = 0 };
            var cell2 = new CellModel() { Row = 1 };
            _cellTracker.AddCell(cell, false);
            _cellTracker.AddCell(cell2, false);
            cells.Add(cell);
            cells.Add(cell2);

            CellFormatEditWindow.MergeCellsDown(cells, _cellTracker);

            Assert.True(cell.IsMerged());
            Assert.True(cell2.IsMerged());
        }

        [Fact]
        public void TwoCellsInSameColumnAndTwoInOtherColumn_MergeCellsDown_TwoMergeParentsCreated()
        {
            CreateInstances();
            var cells = new List<CellModel>();
            var cell = new CellModel() { Row = 0, Column = 0 };
            var cell2 = new CellModel() { Row = 1, Column = 0 };
            var cell3 = new CellModel() { Row = 0, Column = 1 };
            var cell4 = new CellModel() { Row = 1, Column = 1 };
            _cellTracker.AddCell(cell, false);
            _cellTracker.AddCell(cell2, false);
            _cellTracker.AddCell(cell3, false);
            _cellTracker.AddCell(cell4, false);
            cells.Add(cell);
            cells.Add(cell2);
            cells.Add(cell3);
            cells.Add(cell4);

            CellFormatEditWindow.MergeCellsDown(cells, _cellTracker);

            Assert.True(cell.IsMergedParent());
            Assert.True(cell2.IsMergedWith(cell));
            Assert.True(cell3.IsMergedParent());
            Assert.True(cell4.IsMergedWith(cell3));
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.