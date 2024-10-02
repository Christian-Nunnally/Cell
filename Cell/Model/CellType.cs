namespace Cell.Model
{
    /// <summary>
    /// A enum representing the type of a cell.
    /// </summary>
    public enum CellType
    {
        /// <summary>
        /// None type cell, which can not be displayed but still functions.
        /// </summary>
        None = 0,
        /// <summary>
        /// A corner cell, which is the top left cell in a table.
        /// </summary>
        Corner = 1 << 0,
        /// <summary>
        /// A row cell, which is the top cell in a row.
        /// </summary>
        Row = 1 << 1,
        /// <summary>
        /// A column cell, which is the left cell in a column.
        /// </summary>
        Column = 1 << 2,
        /// <summary>
        /// A label cell, which is a cell that displays text.
        /// </summary>
        Label = 1 << 3,
        /// <summary>
        /// A textbox cell, which is a cell that allows the user to input text.
        /// </summary>
        Textbox = 1 << 4,
        /// <summary>
        /// A checkbox cell, which is a cell that allows the user to toggle a value and displays a checkmark.
        /// </summary>
        Checkbox = 1 << 5,
        /// <summary>
        /// A button cell, which is a cell that allows the user to trigger an action.
        /// </summary>
        Button = 1 << 6,
        /// <summary>
        /// A progress cell, which is a cell that displays a progress bar.
        /// </summary>
        Progress = 1 << 7,
        /// <summary>
        /// Dropdown cell, which is a cell that allows the user to select from a list of options.
        /// </summary>
        Dropdown = 1 << 8,
        /// <summary>
        /// List cell, which is a cell that displays a list of items.
        /// </summary>
        List = 1 << 9,
        /// <summary>
        /// Graph cell, which is a cell that displays a graph.
        /// </summary>
        Graph = 1 << 10,
        /// <summary>
        /// Date cell, which is a cell that allows the user to select a date.
        /// </summary>
        Date = 1 << 11,
    }

    /// <summary>
    /// Helpful extensions for <see cref="CellType"/>.
    /// </summary>
    public static class CellTypeExtensions
    {
        /// <summary>
        /// Gets whether this cell is a collection cell, such as a dropdown, list, or graph.
        /// </summary>
        /// <param name="value">The type to check.</param>
        /// <returns>True if the cell is a type that represents a collection instead of a single value.</returns>
        public static bool IsCollection(this CellType value)
        {
            CellType isSpecialType = CellType.Dropdown | CellType.List | CellType.Graph;
            return (value & isSpecialType) != 0;
        }

        /// <summary>
        /// Gets whether this cell is a special cell, such as a corner, row, or column.
        /// </summary>
        /// <param name="value">The type to check.</param>
        /// <returns>True if the cell represents a special cell.</returns>
        public static bool IsSpecial(this CellType value)
        {
            CellType isSpecialType = CellType.Corner | CellType.Row | CellType.Column;
            return (value & isSpecialType) != 0;
        }
    }
}
