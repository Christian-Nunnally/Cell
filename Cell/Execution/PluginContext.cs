using Cell.Data;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.View.ToolWindow;
using Cell.ViewModel.Application;

#pragma warning disable CA1822 // Mark members as static. Justification: Making methods static causes the user to have to type the entire PluginContext type name to call methods, which is not user-friendly.

namespace Cell.Execution
{
    public class PluginContext(ApplicationViewModel application, int index, CellModel? cell = null)
    {
        private CellModel? _cell = cell;
        public const string PluginContextArgumentName = "c";

        private readonly ApplicationViewModel _application = application;
        public EditContext E { get; set; } = new EditContext("");

        public int Index { get; set; } = index;

        public SheetModel[] Sheets => [.. SheetTracker.Instance.Sheets];

        public string[] SheetNames => [.. SheetTracker.Instance.Sheets.Select(x => x.Name)];

        public CellModel GetCell(CellModel cellForSheet, int row, int column) => GetCell(cellForSheet.SheetName, row, column);

        public CellRange GetCell(CellModel cellForSheet, int row, int column, int rowRangeEnd, int columnRangeEnd) => GetCell(cellForSheet.SheetName, row, column, rowRangeEnd, columnRangeEnd);

        public CellModel GetCell(string sheet, int row, int column) => CellTracker.Instance.GetCell(sheet, row, column) ?? CellModel.Empty;

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
            return UserList<T>.GetOrCreate(collection);
        }

        public void GoToCell(CellModel cell)
        {
            _application.GoToSheet(cell.SheetName);
            _application.GoToCell(cell);
        }

        public void GoToSheet(string sheetName)
        {
            _application.GoToSheet(sheetName);
        }

        public void ShowDialog(string text)
        {
            var title = _cell?.UserFriendlyCellName ?? "Function";
            DialogWindow.ShowDialog(title, text);
        }
    }
}
#pragma warning restore CA1822 // Mark members as static