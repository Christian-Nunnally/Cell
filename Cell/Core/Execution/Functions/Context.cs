using Cell.Data;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.ViewModel.Application;

namespace Cell.Execution
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
        public Context(CellTracker cellTracker, UserCollectionLoader userCollectionLoader)
        {
            _cellTracker = cellTracker;
            _userCollectionLoader = userCollectionLoader;
            Cell = null;
        }

        public Context(CellTracker cellTracker, UserCollectionLoader userCollectionLoader, CellModel cell)
        {
            _cellTracker = cellTracker;
            _userCollectionLoader = userCollectionLoader;
            Cell = cell;
            Index = cell.Index;
        }

        // TODO: Somehow always use the cells index because this is confusing. It's currently used to hack efficent sorting in but there is a better way.
        public Context(CellTracker cellTracker, UserCollectionLoader userCollectionLoader, int index)
        {
            _cellTracker = cellTracker;
            _userCollectionLoader = userCollectionLoader;
            Cell = null;
            Index = index;
        }

        /// <summary>
        /// The current cell that the function is running 'in'. This is the same cell that you can access by typing `cell.`.
        /// 
        /// To get other cells use the cell reference format like `A1`, or use the `GetCell` function.
        /// </summary>
        public CellModel? Cell
        {
            get => _cell; set
            {
                _cell = value;
                if (_cell is not null) Index = _cell.Index;
            }
        }

        /// <summary>
        /// Contains information about the edit that caused this function to run.
        /// </summary>
        public EditContext E { get; set; } = new EditContext("");

        /// <summary>
        /// The 'index' of the cell that the function is running in, or the index of the object being sorted by this function. 
        /// 
        /// It is perferred to use `c.index` in functions instead of `cell.Index`, even though they will be the same for non sort functions.
        /// </summary>
        public int Index { get; set; } = 0;

        public CellModel GetCell(CellModel cellForSheet, int row, int column) => GetCell(cellForSheet.SheetName, row, column);

        public CellModel GetCell(string sheet, int row, int column) => _cellTracker.GetCell(sheet, row, column) ?? CellModel.Null;

        public CellRange GetCellRange(CellModel cellForSheet, int row, int column, int rowRangeEnd, int columnRangeEnd) => GetCellRange(cellForSheet.SheetName, row, column, rowRangeEnd, columnRangeEnd);

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

        public UserList<T> GetUserList<T>(string collection) where T : PluginModel, new()
        {
            return UserList<T>.GetOrCreate(collection, _userCollectionLoader);
        }

        public void GoToCell(CellModel cell)
        {
            ApplicationViewModel.Instance.GoToSheet(cell.SheetName);
            ApplicationViewModel.Instance.GoToCell(cell);
        }

        public void GoToSheet(string sheetName)
        {
            ApplicationViewModel.Instance.GoToSheet(sheetName);
        }

        public void ShowDialog(string text)
        {
            var title = Cell?.UserFriendlyCellName ?? "Function";
            DialogFactory.ShowDialog(title, text);
        }
    }
}
