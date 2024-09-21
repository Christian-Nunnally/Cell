using Cell.Data;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.ViewModel.Application;

namespace Cell.Execution
{
    public class PluginContext
    {
        public const string PluginContextArgumentName = "c";
        private readonly CellTracker _cellTracker;
        private readonly UserCollectionLoader _userCollectionLoader;
        private CellModel? cell;
        public PluginContext(CellTracker cellTracker, UserCollectionLoader userCollectionLoader)
        {
            _cellTracker = cellTracker;
            _userCollectionLoader = userCollectionLoader;
            Cell = null;
        }

        public PluginContext(CellTracker cellTracker, UserCollectionLoader userCollectionLoader, CellModel cell)
        {
            _cellTracker = cellTracker;
            _userCollectionLoader = userCollectionLoader;
            Cell = cell;
            Index = cell.Index;
        }

        public PluginContext(CellTracker cellTracker, UserCollectionLoader userCollectionLoader, int index)
        {
            _cellTracker = cellTracker;
            _userCollectionLoader = userCollectionLoader;
            Cell = null;
            Index = index;
        }

        public CellModel? Cell
        {
            get => cell; set
            {
                cell = value;
                if (cell is not null) Index = cell.Index;
            }
        }

        public EditContext E { get; set; } = new EditContext("");

        public int Index { get; set; } = 0;

        public CellModel GetCell(CellModel cellForSheet, int row, int column) => GetCell(cellForSheet.SheetName, row, column);

        public CellRange GetCell(CellModel cellForSheet, int row, int column, int rowRangeEnd, int columnRangeEnd) => GetCell(cellForSheet.SheetName, row, column, rowRangeEnd, columnRangeEnd);

        public CellModel GetCell(string sheet, int row, int column) => _cellTracker.GetCell(sheet, row, column) ?? CellModel.Null;

        public CellRange GetCell(string sheet, int row, int column, int rowRangeEnd, int columnRangeEnd)
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
