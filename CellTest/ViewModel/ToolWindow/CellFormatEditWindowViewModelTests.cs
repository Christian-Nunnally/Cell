using Cell.Core.Data;
using Cell.Model;
using Cell.Core.Persistence;
using Cell.ViewModel.ToolWindow;
using CellTest.TestUtilities;
using System.Collections.ObjectModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CellTest.ViewModel.ToolWindow
{
    public class CellFormatEditWindowViewModelTests
    {
        private CellTracker _cellTracker;
        private DictionaryFileIO _testFileIO;
        private PersistedDirectory _persistedDirectory;
        private ObservableCollection<CellModel> _cellsToEdit;
        private PluginFunctionLoader _pluginFunctionLoader;

        private CellFormatEditWindowViewModel CreateInstance()
        {
            _testFileIO = new DictionaryFileIO();
            _persistedDirectory = new PersistedDirectory("", _testFileIO);
            _cellTracker = new CellTracker();
            _cellsToEdit = [];
            _pluginFunctionLoader = new PluginFunctionLoader(_persistedDirectory);
            return new CellFormatEditWindowViewModel(_cellsToEdit, _cellTracker, _pluginFunctionLoader);
        }

        [Fact]
        public void EmptyListOfCells_MergeCellsDown_Runs()
        {
            var testing = CreateInstance();

            testing.MergeCellsDown();
        }

        [Fact]
        public void SingleCell_MergeCellsDown_DoesNoSetMergedWithIdToSelf()
        {
            var testing = CreateInstance();
            var cells = new List<CellModel>();
            var cell = new CellModel();
            cells.Add(cell);

            testing.MergeCellsDown();

            Assert.False(cell.IsMerged());
        }

        [Fact]
        public void TwoCellsInSameColumn_MergeCellsDown_BothCellsMerged()
        {
            var testing = CreateInstance();
            var cell = new CellModel();
            cell.Location.Row = 0;
            var cell2 = new CellModel();
            cell2.Location.Row = 1;
            _cellTracker.AddCell(cell);
            _cellTracker.AddCell(cell2);
            _cellsToEdit.Add(cell);
            _cellsToEdit.Add(cell2);

            testing.MergeCellsDown();

            Assert.True(cell.IsMerged());
            Assert.True(cell2.IsMerged());
        }

        [Fact]
        public void TwoByTwoGridOfCellsWithAllSelected_MergeCellsDown_TwoMergeParentsCreated()
        {
            var testing = CreateInstance();
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

            testing.MergeCellsDown();

            Assert.True(cell.IsMergedParent());
            Assert.True(cell2.IsMergedWith(cell));
            Assert.True(cell3.IsMergedParent());
            Assert.True(cell4.IsMergedWith(cell3));
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.