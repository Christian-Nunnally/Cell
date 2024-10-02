using Cell.Model;
using System.Collections.ObjectModel;

namespace Cell.Data
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
                _isSelectingEnabled = value;
                UnselectAllCells();
            }
        }

        /// <summary>
        /// Gets an observable collection of the currently selected cells.
        /// </summary>
        public ObservableCollection<CellModel> SelectedCells { get; } = [];

        /// <summary>
        /// Moves the selection by unselecting the current selection and selecting the cell at the specified offset from the current selection.
        /// </summary>
        /// <param name="columnOffset">How many columns the selection should move by. Can be negative to move left.</param>
        /// <param name="rowOffset">How many rows the selection should move by. Can be negative to move up.</param>
        public void MoveSelection(int columnOffset, int rowOffset)
        {
            if (!SelectedCells.Any()) return;
            var singleSelectedCell = SelectedCells.First();
            if (columnOffset > 0) columnOffset += singleSelectedCell.CellsMergedToRight();
            if (rowOffset > 0) rowOffset += singleSelectedCell.CellsMergedBelow();
            var row = singleSelectedCell.Row + rowOffset;
            var column = singleSelectedCell.Column + columnOffset;
            var cellToSelect = _cellTracker.GetCell(singleSelectedCell.SheetName, row, column);
            if (cellToSelect is null) return;
            UnselectAllCells();
            SelectCell(cellToSelect);
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
        /// Adds a cell to the selection.
        /// </summary>
        /// <param name="cell">The cell to add.</param>
        public void SelectCell(CellModel cell)
        {
            if (!IsSelectingEnabled) return;
            SelectedCells.Add(cell);
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
        }
    }
}
