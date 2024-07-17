using Cell.Model;
using Cell.Model.Plugin;
using Cell.Persistence;
using Cell.ViewModel;

#pragma warning disable CA1822 // Mark members as static. Justification: Making methods static causes the user to have to type the entire PluginContext type name to call methods, which is not user-friendly.

namespace Cell.Plugin
{
    public class PluginContext(ApplicationViewModel application)
    {
        private readonly ApplicationViewModel _application = application;

        public string GetCellValue(CellModel cellForSheet, int row, int column)
        {
            return Cells.GetCell(cellForSheet.SheetName, row, column)?.Value ?? string.Empty;
        }

        public string GetText(string id)
        {
            return Cells.GetCell(id).Text;
        }

        public double GetNumber(string id)
        {
            return double.TryParse(Cells.GetCell(id).Text, out var result) ? result : 0;
        }

        public bool GetBoolean(string id)
        {
            return bool.TryParse(Cells.GetCell(id).Text, out var result) && result;
        }

        public void SetCell(string id, string value)
        {
            Cells.GetCell(id).Text = value;
        }

        public void SetCell(string id, double value)
        {
            Cells.GetCell(id).Text = value.ToString();
        }

        public void SetCell(string id, bool value)
        {
            Cells.GetCell(id).Text = value.ToString();
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

        public void GoToCell(string cellId)
        {
            var model = Cells.GetCell(cellId);
            GoToCell(model);
        }

        public IOrderedEnumerable<string> GetList(string collection)
        {
            return UserCollectionLoader.GetCollection(collection).OrderBy(x => 1);
        }

        public void AddToList(string collection, object data)
        {
            var serialized = data.ToString();
            UserCollectionLoader.AddToCollection(collection, serialized ?? "");
        }

        public string GetLastAdded(string collection)
        {
            return UserCollectionLoader.GetCollection(collection).Last();
        }

        public UserList<T> GetUserList<T>(string collection) where T : PluginModel, new()
        {
            return UserList<T>.GetOrCreate(collection);// UserCollectionLoader.GetCollection(collection).Select(PluginModel.FromString<T>).OrderBy(x => 1);
        }

        public T GetLastAdded<T>(string collection) where T : new()
        {
            return PluginModel.FromString<T>(UserCollectionLoader.GetCollection(collection).Last());
        }

        public string[] SheetNames => [.. Cells.GetSheetNames()];
    }
}

#pragma warning restore CA1822 // Mark members as static