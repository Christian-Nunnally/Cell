
namespace Cell.Model
{
    /// <summary>
    /// Extensions for <see cref="CellModel"/>.
    /// </summary>
    public static class CellModelExtensions
    {
        /// <summary>
        /// Determines if the cell is merged with a cell.
        /// </summary>
        /// <param name="cell">The cell to see if it is a merged cell.</param>
        /// <returns>True if the cell is merged with any other cells.</returns>
        public static bool IsMerged(this CellModel cell) => !string.IsNullOrWhiteSpace(cell.MergedWith);

        /// <summary>
        /// Determines if the cell is the parent of a group of merged cells. This is the cell that is displayed, and the other cells are hidden.
        /// </summary>
        /// <param name="cell">The cell to check.</param>
        /// <returns>True if the cell is the merge parent.</returns>
        public static bool IsMergedParent(this CellModel cell) => cell.MergedWith == cell.ID;

        /// <summary>
        /// Determines if the cell is merged with another cell.
        /// </summary>
        /// <param name="cell">The first cell.</param>
        /// <param name="other">The second cell.</param>
        /// <returns>True if the two cells are merged.</returns>
        public static bool IsMergedWith(this CellModel cell, CellModel other) => cell.IsMerged() && cell.MergedWith == other.MergedWith;

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
