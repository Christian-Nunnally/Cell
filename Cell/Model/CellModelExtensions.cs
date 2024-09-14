using Cell.Common;
using Cell.Data;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types.Special;

namespace Cell.Model
{
    public static class CellModelExtensions
    {
        public static int CellsMergedBelow(this CellModel model)
        {
            var count = 0;
            while (model.IsMerged() && (ApplicationViewModel.Instance.CellTracker.GetCell(model.SheetName, model.Row + 1 + count, model.Column)?.IsMergedWith(model) ?? false)) count++;
            return count;
        }

        public static int CellsMergedToRight(this CellModel model)
        {
            var count = 0;
            while (model.IsMerged() && (ApplicationViewModel.Instance.CellTracker.GetCell(model.SheetName, model.Row, model.Column + 1 + count)?.IsMergedWith(model) ?? false)) count++;
            return count;
        }

        public static bool IsMerged(this CellModel model) => !string.IsNullOrWhiteSpace(model.MergedWith);

        public static bool IsMergedParent(this CellModel model) => model.MergedWith == model.ID;

        public static bool IsMergedWith(this CellModel model, CellModel other) => model.IsMerged() && model.MergedWith == other.MergedWith;

        public static void EnsureIndexStaysCumulativeWhenRemoving(this CellModel removingCell, CellModel? nextCell, CellTracker cellTracker)
        {
            if (nextCell is null) return;
            if (nextCell.CellType.IsSpecial()) return;
            else FixIndexOfCellsAfterRemovingCell(removingCell, nextCell, cellTracker);
        }

        public static void EnsureIndexStaysCumulativeBetweenNeighborsWhenAdding(this CellModel cellModel, CellModel? firstNeighbor, CellModel? secondNeighbor, CellTracker cellTracker)
        {
            var isThereAFirstNeighbor = firstNeighbor is not null && !firstNeighbor.CellType.IsSpecial();
            var isThereASecondNeighbor = secondNeighbor is not null && !secondNeighbor.CellType.IsSpecial();

            if (!isThereAFirstNeighbor && !isThereASecondNeighbor) return;
            if (isThereAFirstNeighbor && !isThereASecondNeighbor) cellModel.Index = firstNeighbor!.Index + 1;
            else if (isThereASecondNeighbor) FixIndexOfCellsAfterAddedCell(cellModel, secondNeighbor!, cellTracker);
        }

        private static void FixIndexOfCellsAfterRemovingCell(CellModel removedCell, CellModel nextCell, CellTracker cellTracker)
            {
            var xDifference = nextCell.Column - removedCell.Column;
            var yDifference = nextCell.Row - removedCell.Row;
            if (xDifference > 1 || yDifference > 1 || xDifference < 0 || yDifference < 0) throw new CellError("FixIndexOfCellsAfterRemovingCell must be called with cells that are next to each other, and added cell must above or to the right of next cell");

            var searchingCell = nextCell;
            int i = removedCell.Index + 1;
            while (searchingCell != null && searchingCell.Index == i)
            {
                searchingCell.Index--;
                i++;
                searchingCell = cellTracker.GetCell(nextCell.SheetName, searchingCell.Row + yDifference, searchingCell.Column + xDifference);
            }
        }

        private static void FixIndexOfCellsAfterAddedCell(CellModel addedCell, CellModel nextCell, CellTracker cellTracker)
        {
            var xDifference = nextCell.Column - addedCell.Column;
            var yDifference = nextCell.Row - addedCell.Row;
            if (xDifference > 1 || yDifference > 1 || xDifference < 0 || yDifference < 0) throw new CellError("FixIndexOfCellsAfterAddedCell must be called with cells that are next to each other, and added cell must above or to the right of next cell");

            addedCell.Index = nextCell.Index;
            var searchingCell = nextCell;
            int i = nextCell.Index;
            while (searchingCell != null && searchingCell.Index == i)
            {
                searchingCell.Index++;
                i++;
                searchingCell = cellTracker.GetCell(nextCell.SheetName, searchingCell.Row + yDifference, searchingCell.Column + xDifference);
            }
        }

        public static void MergeCellIntoCellsIfTheyWereMerged(this CellModel cellToMerge, CellModel? firstCell, CellModel? secondCell)
        {
            if (firstCell == null || secondCell == null) return;
            if (firstCell.IsMergedWith(secondCell)) cellToMerge.MergedWith = firstCell.MergedWith;
        }

        public static string GetName(this CellModel cell) => $"{ColumnCellViewModel.GetColumnName(cell.Column)}{cell.Row}";
    }
}
