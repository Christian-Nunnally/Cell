using Cell.Data;

namespace Cell.Model
{
    /// <summary>
    /// Factory for creating sheets pre populated with cells.
    /// </summary>
    public class SheetFactory
    {
        /// <summary>
        /// Creates a new sheet with the given name and initial rows and columns.
        /// </summary>
        /// <param name="sheetName">The name to give the new sheet.</param>
        /// <param name="initialRows">The initial number of rows to generate cells for.</param>
        /// <param name="initialColumns">The initial number of columns to generate cells for.</param>
        /// <param name="cellTracker">The cell tracker to add the new cells to.</param>
        public static void CreateSheet(string sheetName, int initialRows, int initialColumns, CellTracker cellTracker)
        {
            CellModelFactory.Create(0, 0, CellType.Corner, sheetName, cellTracker);

            for (var i = 0; i < initialRows; i++)
            {
                CellModelFactory.Create(i + 1, 0, CellType.Row, sheetName, cellTracker);
            }
            for (var i = 0; i < initialColumns; i++)
            {
                CellModelFactory.Create(0, i + 1, CellType.Column, sheetName, cellTracker);
            }
            for (var i = 0; i < initialRows; i++)
            {
                for (var j = 0; j < initialColumns; j++)
                {
                    CellModelFactory.Create(i + 1, j + 1, CellType.Label, sheetName, cellTracker);
                }
            }
        }
    }
}
