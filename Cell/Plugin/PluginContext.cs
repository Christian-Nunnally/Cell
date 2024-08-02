using Cell.Data;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.ViewModel;

#pragma warning disable CA1822 // Mark members as static. Justification: Making methods static causes the user to have to type the entire PluginContext type name to call methods, which is not user-friendly.

namespace Cell.Plugin
{
    public class PluginContext(ApplicationViewModel application, int index)
    {
        private readonly ApplicationViewModel _application = application;

        public EditContext E { get; set; } = new EditContext("");

        public int Index { get; set; } = index;

        public CellModel GetCell(CellModel cellForSheet, int row, int column) => GetCell(cellForSheet.SheetName, row, column);

        public CellRange GetCell(CellModel cellForSheet, int row, int column, int rowRangeEnd, int columnRangeEnd) => GetCell(cellForSheet.SheetName, row, column, rowRangeEnd, columnRangeEnd);

        public CellModel GetCell(string sheet, int row, int column) => Cells.Instance.GetCell(sheet, row, column) ?? CellModel.Empty;

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

        public void GoToSheet(string sheetName)
        {
            _application.GoToSheet(sheetName);
        }

        public void GoToCell(CellModel cell)
        {
            _application.GoToSheet(cell.SheetName);
            _application.GoToCell(cell);
        }

        public UserList<T> GetUserList<T>(string collection) where T : PluginModel, new()
        {
            return UserList<T>.GetOrCreate(collection);
        }

        public string[] SheetNames => [.. Cells.Instance.SheetNames];
    }
}

#pragma warning restore CA1822 // Mark members as static