using Cell.Core.Data.Tracker;
using Cell.Model;
using System.Collections.ObjectModel;

namespace Cell.Core.Data
{
    /// <summary>
    /// Class that allows cells to be selected, the selection to be manipulated, and the selected cells to be retrieved.
    /// </summary>
    public class CellSelector
    {
        private readonly CellTracker _cellTracker;
        private bool _isSelectingEnabled = true;
        /// <summary>
        /// Creates a new instance of <see cref="CellSelector"/>.
        /// </summary>
        /// <param name="cellTracker">The cell tracker used as the source of cells for this selector.</param>
        public CellSelector(CellTracker cellTracker)
        {
            _cellTracker = cellTracker;
        }

        /// <summary>
        /// Gets or sets whether cells can be selected with this selector.
        /// </summary>
        public bool IsSelectingEnabled
        {
            get => _isSelectingEnabled; internal set
            {
                if (_isSelectingEnabled == value) return;
                _isSelectingEnabled = value;
                UnselectAllCells();
            }
        }

        /// <summary>
        /// Gets an observable collection of the currently selected cells.
        /// </summary>
        public ObservableCollection<CellModel> SelectedCells { get; } = [];
        private HashSet<CellModel> SelectedCellsHashSet { get; } = [];

        /// <summary>
        /// A null cell selector.
        /// </summary>
        public readonly static CellSelector Null = new (null!);

        /// <summary>
        /// Moves the selection by unselecting the current selection and selecting the cell at the specified offset from the current selection.
        /// </summary>
        /// <param name="columnOffset">How many columns the selection should move by. Can be negative to move left.</param>
        /// <param name="rowOffset">How many rows the selection should move by. Can be negative to move up.</param>
        public void MoveSelection(int columnOffset, int rowOffset)
        {
            var cellsToSelect = GetCellsOffsetFromCurrentlySelectedCells(ref columnOffset, ref rowOffset);
            if (cellsToSelect.Count != 0) UnselectAllCells();
            SelectCells(cellsToSelect);
        }

        private List<CellModel> GetCellsOffsetFromCurrentlySelectedCells(ref int columnOffset, ref int rowOffset)
        {
            var cellsToSelect = new List<CellModel>();
            foreach (var selectedCell in SelectedCells)
            {
                if (columnOffset > 0) columnOffset += CountCellsMergedToRight(selectedCell);
                if (rowOffset > 0) rowOffset += CountCellsMergedBelow(selectedCell);
                var row = selectedCell.Location.Row + rowOffset;
                var column = selectedCell.Location.Column + columnOffset;
                var cellToSelect = _cellTracker.GetCell(selectedCell.Location.SheetName, row, column);
                if (cellToSelect is not null) cellsToSelect.Add(cellToSelect);
            }

            return cellsToSelect;
        }

        /// <summary>
        /// Adds cells to the current selection that are next to the currently selected cells in the with the given offset.
        /// </summary>
        /// <param name="columnOffset">How many columns the selection should move by. Can be negative to move left.</param>
        /// <param name="rowOffset">How many rows the selection should move by. Can be negative to move up.</param>
        public void AddToSelection(int columnOffset, int rowOffset)
        {
            var cellsToSelect = GetCellsOffsetFromCurrentlySelectedCells(ref columnOffset, ref rowOffset);
            SelectCells(cellsToSelect);
        }

        private int CountCellsMergedBelow(CellModel model)
        {
            var count = 0;
            while (model.IsMerged() && (_cellTracker.GetCell(model.Location.WithRowOffset(1 + count))?.IsMergedWith(model) ?? false)) count++;
            return count;
        }

        private int CountCellsMergedToRight(CellModel model)
        {
            var count = 0;
            while (model.IsMerged() && (_cellTracker.GetCell(model.Location.WithColumnOffset(1 + count))?.IsMergedWith(model) ?? false)) count++;
            return count;
        }

        /// <summary>
        /// Moves the selection down by unselecting the current selection and selecting the cell below the current selection.
        /// </summary>
        public void MoveSelectionDown() => MoveSelection(0, 1);

        /// <summary>
        /// Moves the selection left by unselecting the current selection and selecting the cell to the left of the current selection.
        /// </summary>
        public void MoveSelectionLeft() => MoveSelection(-1, 0);

        /// <summary>
        /// Moves the selection right by unselecting the current selection and selecting the cell to the right of the current selection.
        /// </summary>
        public void MoveSelectionRight() => MoveSelection(1, 0);

        /// <summary>
        /// Moves the selection up by unselecting the current selection and selecting the cell above the current selection.
        /// </summary>
        public void MoveSelectionUp() => MoveSelection(0, -1);

        /// <summary>
        /// Adds to the selection down, selecting the cells below the current selection.
        /// </summary>
        public void AddToSelectionDown() => AddToSelection(0, 1);

        /// <summary>
        /// Adds to the selection left, selecting the cells to the left of the current selection.
        /// </summary>
        public void AddToSelectionLeft() => AddToSelection(-1, 0);

        /// <summary>
        /// Adds to the selection right, selecting the cells to the right of the current selection.
        /// </summary>
        public void AddToSelectionRight() => AddToSelection(1, 0);

        /// <summary>
        /// Adds to the selection up, selecting the cells above the current selection.
        /// </summary>
        public void AddToSelectionUp() => AddToSelection(0, -1);

        /// <summary>
        /// Adds a cell to the selection.
        /// </summary>
        /// <param name="cell">The cell to add.</param>
        public void SelectCell(CellModel cell)
        {
            if (!IsSelectingEnabled) return;
            if(SelectedCellsHashSet.Contains(cell)) return;
            SelectedCellsHashSet.Add(cell);
            SelectedCells.Add(cell);
        }

        /// <summary>
        /// Adds a list of cells to the selection.
        /// </summary>
        /// <param name="cells">The cells to select.</param>
        public void SelectCells(IEnumerable<CellModel> cells)
        {
            foreach (var cell in cells) SelectCell(cell);
        }

        /// <summary>
        /// Removes all cells from the selection.
        /// </summary>
        public void UnselectAllCells()
        {
            foreach (var cell in SelectedCells.ToList()) UnselectCell(cell);
        }

        /// <summary>
        /// Removes a cell from the selection.
        /// </summary>
        /// <param name="cell">The cell to remove.</param>
        public void UnselectCell(CellModel cell)
        {
            SelectedCells.Remove(cell);
            SelectedCellsHashSet.Remove(cell);
        }
    }
}
