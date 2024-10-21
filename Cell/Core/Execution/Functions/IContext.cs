using Cell.Core.Data;
using Cell.Model.Plugin;
using Cell.Model;

namespace Cell.Core.Execution.Functions
{
    /// <summary>
    /// Provides contextual information to a function, such as what the old value of a cell was before the function triggered.
    /// 
    /// Also provides access to functions that allow you to do things, such as get a cell, show a dialog, or go to a cell.
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// Contains information about the edit that caused this function to run.
        /// </summary>
        public EditContext E { get; set; }

        /// <summary>
        /// The 'index' of the cell that the function is running in.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The current cell that the function is running 'in'. This is the same cell that you can access by typing `cell.`.
        /// </summary>
        public CellModel ContextCell { get; set; }

        /// <summary>
        /// Gets a cell from the given sheet, at the given row and column.
        /// </summary>
        /// <param name="cellForSheet">The cell whos sheet should be searched for the returned cell.</param>
        /// <param name="row">The row the cell is at.</param>
        /// <param name="column">The column the cell is at.</param>
        /// <returns>The found cell, or a null cell if no real cell exists there.</returns>
        public CellModel GetCell(CellModel cellForSheet, int row, int column);

        /// <summary>
        /// Gets a cell from the given sheet, at the given row and column.
        /// </summary>
        /// <param name="sheet">The sheet name to get the cell from.</param>
        /// <param name="row">The row the cell is at.</param>
        /// <param name="column">The column the cell is at.</param>
        /// <returns>The found cell, or a null cell if no real cell exists there.</returns>
        public CellModel GetCell(string sheet, int row, int column);

        /// <summary>
        /// Gets a range of cells from the given sheet, starting at the given row and column, and ending at the given row and column.
        /// </summary>
        /// <param name="cellForSheet">A cell to use to get the sheetname the range should come from from.</param>
        /// <param name="row">The starting row of the range, inclusive.</param>
        /// <param name="column">The starting column of the range, inclusive.</param>
        /// <param name="rowRangeEnd">The end row of the range, inclusive.</param>
        /// <param name="columnRangeEnd">The end column of the range inclusive.</param>
        /// <returns>A cell range object.</returns>
        public CellRange GetCellRange(CellModel cellForSheet, int row, int column, int rowRangeEnd, int columnRangeEnd) => GetCellRange(cellForSheet.Location.SheetName, row, column, rowRangeEnd, columnRangeEnd);

        /// <summary>
        /// Gets a range of cells from the given sheet, starting at the given row and column, and ending at the given row and column.
        /// </summary>
        /// <param name="sheet">The sheet to get the cell range from.</param>
        /// <param name="row">The starting row of the range, inclusive.</param>
        /// <param name="column">The starting column of the range, inclusive.</param>
        /// <param name="rowRangeEnd">The end row of the range, inclusive.</param>
        /// <param name="columnRangeEnd">The end column of the range inclusive.</param>
        /// <returns>A cell range object.</returns>
        public CellRange GetCellRange(string sheet, int row, int column, int rowRangeEnd, int columnRangeEnd);

        /// <summary>
        /// Gets the user defined list of objects with the given name.
        /// </summary>
        /// <typeparam name="T">The type of the items in the list.</typeparam>
        /// <param name="collection">The name of the list.</param>
        /// <returns>The user collection with the given name.</returns>
        public UserList<T> GetUserList<T>(string collection) where T : PluginModel, new();

        /// <summary>
        /// Goes to the given cell in the UI. Will change the sheet if the cell is on a different sheet.
        /// </summary>
        /// <param name="cell">The cell to move to.</param>
        public void GoToCell(CellModel cell);

        /// <summary>
        /// Opens the given sheet in the UI.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to open.</param>
        public void GoToSheet(string sheetName);

        /// <summary>
        /// Shows a dialog to the user with the given text.
        /// </summary>
        /// <param name="text">The text to show in the dialog window.</param>
        public void ShowDialog(string text);
    }
}
