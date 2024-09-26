using Cell.Model;
using System.Collections.ObjectModel;

namespace Cell.Data
{
    public class CellSelector
    {
        private readonly CellTracker _cellTracker;
        private bool _isSelectingEnabled = true;

        public ObservableCollection<CellModel> SelectedCells { get; } = [];
        public bool IsSelectingEnabled
        {
            get => _isSelectingEnabled; internal set
            {
                _isSelectingEnabled = value;
                UnselectAllCells();
            }
        }

        public CellSelector(CellTracker cellTracker)
        {
            _cellTracker = cellTracker;
        }

        public void SelectCell(CellModel cell)
        {
            if (!IsSelectingEnabled) return;
            SelectedCells.Add(cell);
        }

        public void UnselectAllCells()
        {
            foreach (var cell in SelectedCells.ToList()) UnselectCell(cell);
        }

        public void UnselectCell(CellModel cell)
        {
            SelectedCells.Remove(cell);
        }

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

        public void MoveSelectionDown() => MoveSelection(0, 1);

        public void MoveSelectionLeft() => MoveSelection(-1, 0);

        public void MoveSelectionRight() => MoveSelection(1, 0);

        public void MoveSelectionUp() => MoveSelection(0, -1);
    }
}
