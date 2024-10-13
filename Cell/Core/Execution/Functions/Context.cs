﻿using Cell.Core.Data;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.Core.Persistence;
using Cell.ViewModel.Application;

namespace Cell.Core.Execution
{
    /// <summary>
    /// Provides contextual information to a function, such as what the old value of a cell was before the function triggered.
    /// 
    /// Also provides access to functions that allow you to do things, such as get a cell, show a dialog, or go to a cell.
    /// </summary>
    public class Context
    {
        /// <summary>
        /// The name of the argument that contains the context object in a plugin function (usually "c").
        /// </summary>
        public const string PluginContextArgumentName = "c";
        private readonly CellTracker _cellTracker;
        private readonly UserCollectionLoader _userCollectionLoader;
        private CellModel? _cell;
        /// <summary>
        /// Creates a new instance of the <see cref="Context"/> class with the cell context set to null.
        /// </summary>
        /// <param name="cellTracker">The cell tracker used to provide cell access to the function.</param>
        /// <param name="userCollectionLoader">The collection loader used to provide collection access to the function.</param>
        public Context(CellTracker cellTracker, UserCollectionLoader userCollectionLoader)
        {
            _cellTracker = cellTracker;
            _userCollectionLoader = userCollectionLoader;
            Cell = null;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Context"/> class with the context set to the given cell.
        /// </summary>
        /// <param name="cellTracker">The cell tracker used to provide cell access to the function.</param>
        /// <param name="userCollectionLoader">The collection loader used to provide collection access to the function.</param>
        /// <param name="cell">The context cell.</param>
        public Context(CellTracker cellTracker, UserCollectionLoader userCollectionLoader, CellModel cell)
        {
            _cellTracker = cellTracker;
            _userCollectionLoader = userCollectionLoader;
            Cell = cell;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Context"/> class for getting the result of a sort function.
        /// </summary>
        /// <param name="cellTracker">The cell tracker used to provide cell access to the function.</param>
        /// <param name="userCollectionLoader">The collection loader used to provide collection access to the function.</param>
        /// <param name="sortIndex">Index used to sort.</param>
        public Context(CellTracker cellTracker, UserCollectionLoader userCollectionLoader, int sortIndex)
        {
            _cellTracker = cellTracker;
            _userCollectionLoader = userCollectionLoader;
            Cell = null;
            SortIndex = sortIndex;
        }

        /// <summary>
        /// The current cell that the function is running 'in'. This is the same cell that you can access by typing `cell.`.
        /// 
        /// To get other cells use the cell reference format like `A1`, or use the `GetCell` function.
        /// </summary>
        public CellModel? Cell
        {
            get => _cell;
            set => _cell = value;
        }

        /// <summary>
        /// Contains information about the edit that caused this function to run.
        /// </summary>
        public EditContext E { get; set; } = new EditContext("");

        /// <summary>
        /// The 'index' of the cell that the function is running in.
        /// </summary>
        public int Index => Cell?.Index ?? 0;

        /// <summary>
        /// The index of the object being sorted by this function call. This is used in sort and filter functions like `return list[c.SortIndex].SortProperty;`
        /// </summary>
        public int SortIndex { get; set; } = 0;

        /// <summary>
        /// Gets a cell from the given sheet, at the given row and column.
        /// </summary>
        /// <param name="cellForSheet">The cell whos sheet should be searched for the returned cell.</param>
        /// <param name="row">The row the cell is at.</param>
        /// <param name="column">The column the cell is at.</param>
        /// <returns>The found cell, or a null cell if no real cell exists there.</returns>
        public CellModel GetCell(CellModel cellForSheet, int row, int column) => GetCell(cellForSheet.Location.SheetName, row, column);

        /// <summary>
        /// Gets a cell from the given sheet, at the given row and column.
        /// </summary>
        /// <param name="sheet">The sheet name to get the cell from.</param>
        /// <param name="row">The row the cell is at.</param>
        /// <param name="column">The column the cell is at.</param>
        /// <returns>The found cell, or a null cell if no real cell exists there.</returns>
        public CellModel GetCell(string sheet, int row, int column) => _cellTracker.GetCell(sheet, row, column) ?? CellModel.Null;

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
        public CellRange GetCellRange(string sheet, int row, int column, int rowRangeEnd, int columnRangeEnd)
        {
            var cells = new List<CellModel>();
            for (var r = row; r <= rowRangeEnd; r++)
            {
                for (var c = column; c <= columnRangeEnd; c++)
                {
                    var cell = GetCell(sheet, r, c);
                    if (cell is not null) cells.Add(GetCell(sheet, r, c));
                }
            }
            return new CellRange(cells);
        }

        /// <summary>
        /// Gets the user defined list of objects with the given name.
        /// </summary>
        /// <typeparam name="T">The type of the items in the list.</typeparam>
        /// <param name="collection">The name of the list.</param>
        /// <returns>The user collection with the given name.</returns>
        public UserList<T> GetUserList<T>(string collection) where T : PluginModel, new()
        {
            return UserList<T>.GetOrCreate(collection, _userCollectionLoader);
        }

        /// <summary>
        /// Goes to the given cell in the UI. Will change the sheet if the cell is on a different sheet.
        /// </summary>
        /// <param name="cell">The cell to move to.</param>
        public void GoToCell(CellModel cell)
        {
            ApplicationViewModel.Instance.GoToSheet(cell.Location.SheetName);
            ApplicationViewModel.Instance.GoToCell(cell);
        }

        /// <summary>
        /// Opens the given sheet in the UI.
        /// </summary>
        /// <param name="sheetName">The name of the sheet to open.</param>
        public void GoToSheet(string sheetName)
        {
            ApplicationViewModel.Instance.GoToSheet(sheetName);
        }

        /// <summary>
        /// Shows a dialog to the user with the given text.
        /// </summary>
        /// <param name="text">The text to show in the dialog window.</param>
        public void ShowDialog(string text)
        {
            var title = Cell?.Location.UserFriendlyLocationString ?? "Function";
            DialogFactory.ShowDialog(title, text);
        }
    }
}
