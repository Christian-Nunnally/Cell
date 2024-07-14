using Cell.Model;
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
            return Cells.GetCellModelsForSheet(cellForSheet.SheetName).FirstOrDefault(x => x.Row == row && x.Column == column)?.Value ?? string.Empty;
        }

        public string GetText(string id)
        {
            return Cells.GetCellModel(id).Text;
        }

        public double GetNumber(string id)
        {
            return double.TryParse(Cells.GetCellModel(id).Text, out var result) ? result : 0;
        }

        public bool GetBoolean(string id)
        {
            return bool.TryParse(Cells.GetCellModel(id).Text, out var result) && result;
        }

        public void SetCell(string id, string value)
        {
            Cells.GetCellModel(id).Text = value;
        }

        public void SetCell(string id, double value)
        {
            Cells.GetCellModel(id).Text = value.ToString();
        }

        public void SetCell(string id, bool value)
        {
            Cells.GetCellModel(id).Text = value.ToString();
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
            var model = Cells.GetCellModel(cellId);
            GoToCell(model);
        }

        public IOrderedEnumerable<string> GetData(string collection)
        {
            return UserCollectionLoader.GetCollection(collection).OrderBy(x => 1);
        }

        public void WriteData(string collection, string data)
        {
            UserCollectionLoader.AddToCollection(collection, data);
        }
    }
}

#pragma warning restore CA1822 // Mark members as static