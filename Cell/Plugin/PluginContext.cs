using Cell.Data;
using Cell.Model;
using Cell.Model.Plugin;
using Cell.ViewModel;

#pragma warning disable CA1822 // Mark members as static. Justification: Making methods static causes the user to have to type the entire PluginContext type name to call methods, which is not user-friendly.

namespace Cell.Plugin
{
    public class PluginContext(ApplicationViewModel application)
    {
        private readonly ApplicationViewModel _application = application;

        public CellModel GetCell(CellModel cellForSheet, int row, int column)
        {
            return Cells.GetCell(cellForSheet.SheetName, row, column) ?? CellModel.Empty;
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

        public string[] SheetNames => [.. Cells.GetSheetNames()];
    }
}

#pragma warning restore CA1822 // Mark members as static