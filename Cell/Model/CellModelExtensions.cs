using Cell.Common;
using Cell.Data;

namespace Cell.Model
{
    /// <summary>
    /// Extensions for <see cref="CellModel"/>.
    /// </summary>
    public static class CellModelExtensions
    {
        public static void EnsureIndexStaysCumulativeBetweenNeighborsWhenAdding(this CellModel cellModel, CellModel? firstNeighbor, CellModel? secondNeighbor, CellTracker cellTracker)
        {
            var isThereAFirstNeighbor = firstNeighbor is not null && !firstNeighbor.CellType.IsSpecial();
            var isThereASecondNeighbor = secondNeighbor is not null && !secondNeighbor.CellType.IsSpecial();

            if (!isThereAFirstNeighbor && !isThereASecondNeighbor) return;
            if (isThereAFirstNeighbor && !isThereASecondNeighbor) cellModel.Index = firstNeighbor!.Index + 1;
            else if (isThereASecondNeighbor) FixIndexOfCellsAfterAddedCell(cellModel, secondNeighbor!, cellTracker);
        }

        public static void EnsureIndexStaysCumulativeWhenRemoving(this CellModel removingCell, CellModel? nextCell, CellTracker cellTracker)
        {
            if (nextCell is null) return;
            if (nextCell.CellType.IsSpecial()) return;
            else FixIndexOfCellsAfterRemovingCell(removingCell, nextCell, cellTracker);
        }

        public static bool IsMerged(this CellModel model) => !string.IsNullOrWhiteSpace(model.MergedWith);

        public static bool IsMergedParent(this CellModel model) => model.MergedWith == model.ID;

        public static bool IsMergedWith(this CellModel model, CellModel other) => model.IsMerged() && model.MergedWith == other.MergedWith;

        public static void MergeCellIntoCellsIfTheyWereMerged(this CellModel cellToMerge, CellModel? firstCell, CellModel? secondCell)
        {
            if (firstCell == null || secondCell == null) return;
            if (firstCell.IsMergedWith(secondCell)) cellToMerge.MergedWith = firstCell.MergedWith;
        }

        private static void FixIndexOfCellsAfterAddedCell(CellModel addedCell, CellModel nextCell, CellTracker cellTracker)
        {
            var xDifference = nextCell.Location.Column - addedCell.Location.Column;
            var yDifference = nextCell.Location.Row - addedCell.Location.Row;
            if (xDifference > 1 || yDifference > 1 || xDifference < 0 || yDifference < 0) throw new CellError("FixIndexOfCellsAfterAddedCell must be called with cells that are next to each other, and added cell must above or to the right of next cell");

            addedCell.Index = nextCell.Index;
            var searchingCell = nextCell;
            int i = nextCell.Index;
            while (searchingCell != null && searchingCell.Index == i)
            {
                searchingCell.Index++;
                i++;
                searchingCell = cellTracker.GetCell(nextCell.Location.SheetName, searchingCell.Location.Row + yDifference, searchingCell.Location.Column + xDifference);
            }
        }

        private static void FixIndexOfCellsAfterRemovingCell(CellModel removedCell, CellModel nextCell, CellTracker cellTracker)
        {
            var xDifference = nextCell.Location.Column - removedCell.Location.Column;
            var yDifference = nextCell.Location.Row - removedCell.Location.Row;
            if (xDifference > 1 || yDifference > 1 || xDifference < 0 || yDifference < 0) throw new CellError("FixIndexOfCellsAfterRemovingCell must be called with cells that are next to each other, and added cell must above or to the right of next cell");

            var searchingCell = nextCell;
            int i = removedCell.Index + 1;
            while (searchingCell != null && searchingCell.Index == i)
            {
                searchingCell.Index--;
                i++;
                searchingCell = cellTracker.GetCell(nextCell.Location.SheetName, searchingCell.Location.Row + yDifference, searchingCell.Location.Column + xDifference);
            }
        }

        /// <summary>
        /// Sets both the border and the content border of the cell to the same color.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="color">The color to set the borders to.</param>
        public static void SetBorders(this CellModel cell, string color)
        {
            cell.Style.BorderColor = color;
            cell.Style.ContentBorderColor = color;
        }

        /// <summary>
        /// Sets the color of both the backgrounds and the borders of the cell at the same time.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="color">The color to set the entire cell to (excluding the foreground)</param>
        public static void SetColor(this CellModel cell, string color)
        {
            cell.SetBackgrounds(color);
            cell.SetBorders(color);
        }

        /// <summary>
        /// Sets both the background and the content background of the cell to the same color.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="color">The color to set the backgrounds to.</param>
        public static void SetBackgrounds(this CellModel cell, string color)
        {
            cell.Style.BackgroundColor = color;
            cell.Style.ContentBackgroundColor = color;
        }
    }
}
