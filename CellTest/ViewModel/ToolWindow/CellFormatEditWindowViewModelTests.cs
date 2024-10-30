using Cell.Core.Data.Tracker;
using Cell.Model;
using Cell.ViewModel.ToolWindow;
using System.Collections.ObjectModel;

namespace CellTest.ViewModel.ToolWindow
{
    public class CellFormatEditWindowViewModelTests
    {
        private readonly CellTracker _cellTracker;
        private readonly ObservableCollection<CellModel> _cellsToEdit;
        private readonly FunctionTracker _functionTracker;
        private readonly CellFormatEditWindowViewModel _testing;

        public CellFormatEditWindowViewModelTests()
        {
            _cellTracker = new CellTracker();
            _cellsToEdit = [];
            _functionTracker = new FunctionTracker();
            _testing = new CellFormatEditWindowViewModel(_cellsToEdit, _cellTracker, _functionTracker);
        }

        [Fact]
        public void EmptyListOfCells_MergeCellsDown_Runs()
        {
            _testing.MergeCellsDown();
        }

        [Fact]
        public void SingleCell_MergeCellsDown_DoesNoSetMergedWithIdToSelf()
        {
            var cells = new List<CellModel>();
            var cell = new CellModel();
            cells.Add(cell);

            _testing.MergeCellsDown();

            Assert.False(cell.IsMerged());
        }

        [Fact]
        public void TwoCellsInSameColumn_MergeCellsDown_BothCellsMerged()
        {
            var cell = new CellModel();
            cell.Location.Row = 0;
            var cell2 = new CellModel();
            cell2.Location.Row = 1;
            _cellTracker.AddCell(cell);
            _cellTracker.AddCell(cell2);
            _cellsToEdit.Add(cell);
            _cellsToEdit.Add(cell2);

            _testing.MergeCellsDown();

            Assert.True(cell.IsMerged());
            Assert.True(cell2.IsMerged());
        }

        [Fact]
        public void TwoByTwoGridOfCellsWithAllSelected_MergeCellsDown_TwoMergeParentsCreated()
        {
            var cell = new CellModel();
            cell.Location.Row = 0;
            cell.Location.Column = 0;
            var cell2 = new CellModel();
            cell2.Location.Row = 1;
            cell2.Location.Column = 0;
            var cell3 = new CellModel();
            cell3.Location.Row = 0;
            cell3.Location.Column = 1;
            var cell4 = new CellModel();
            cell4.Location.Row = 1;
            cell4.Location.Column = 1;
            _cellTracker.AddCell(cell);
            _cellTracker.AddCell(cell2);
            _cellTracker.AddCell(cell3);
            _cellTracker.AddCell(cell4);
            _cellsToEdit.Add(cell);
            _cellsToEdit.Add(cell2);
            _cellsToEdit.Add(cell3);
            _cellsToEdit.Add(cell4);

            _testing.MergeCellsDown();

            Assert.True(cell.IsMergedParent());
            Assert.True(cell2.IsMergedWith(cell));
            Assert.True(cell3.IsMergedParent());
            Assert.True(cell4.IsMergedWith(cell3));
        }
    }
}
