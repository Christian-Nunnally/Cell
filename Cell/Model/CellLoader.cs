using Cell.ViewModel;
using System.IO;
namespace Cell.Model
{
    static class CellLoader
    {
        public static string DefaultSaveLocation = "C:\\Users\\chris\\Documents\\CellSaves";
        private static bool haveCellsLoaded = false;
        private static Dictionary<string, Dictionary<string, CellModel>> cells = new();

        public static void LoadCells(string saveDirectory)
        {
            PluginFunctionLoader.LoadPlugins();
            var sheetsPath = Path.Combine(saveDirectory, "Sheets");
            if (Directory.Exists(sheetsPath))
            {
                foreach (var directory in Directory.GetDirectories(sheetsPath))
                {
                    LoadSheet(directory);
                }
            }
            haveCellsLoaded = true;
        }

        private static void LoadSheet(string directory)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                LoadCell(file);
            }
        }

        private static CellModel LoadCell(string file)
        {
            var cell = CellModel.DeserializeModel(File.ReadAllText(file));
            AddCell(cell);
            return cell;
        }

        public static void AddCell(CellModel cellModel)
        {
            if (cells.TryGetValue(cellModel.SheetName, out var cellDictionary))
            {
                cellDictionary.Add(cellModel.Id, cellModel);
            }
            else
            {
                cells.Add(cellModel.SheetName, new Dictionary<string, CellModel> { { cellModel.Id, cellModel } });
            }
        }

        public static void RemoveCell(CellModel cellModel)
        {
            if (cells.TryGetValue(cellModel.SheetName, out var cellDictionary))
            {
                cellDictionary.Remove(cellModel.Id);
            }
        }

        public static void SaveCells(string saveDirectory)
        {
            foreach (var sheet in cells.Keys)
            {
                SaveSheet(saveDirectory, sheet);
            }
        }

        private static void SaveSheet(string saveDirectory, string sheet)
        {
            foreach (var cell in cells[sheet].Values)
            {
                SaveCell(saveDirectory, cell);
            }
        }

        private static void SaveCell(string saveDirectory, CellModel cell)
        {
            var directory = Path.Combine(saveDirectory, "Sheets", cell.SheetName);
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, cell.Id), CellModel.SerializeModel(cell));
        }

        public static CellModel GetCellModel(string sheetName, string cellId)
        {
            if (cells.TryGetValue(sheetName, out var cellDictionary))
            {
                if (cellDictionary.TryGetValue(cellId, out var cellModel))
                {
                    return cellModel;
                }
            }
            return null;
        }

        public static List<CellModel> GetCellModelsForSheet(string sheetName)
        {
            EnsureCellsAreLoaded();
            if (cells.TryGetValue(sheetName, out var cellDictionary))
            {   
                return cellDictionary.Values.ToList();
            }
            return new List<CellModel>();
        }

        private static void EnsureCellsAreLoaded()
        {
            if (!haveCellsLoaded) LoadCells(DefaultSaveLocation);
        }

        public static List<CellViewModel> GetCellViewModelsForSheet(SheetViewModel sheet)
        {
            var temp = GetCellModelsForSheet(sheet.SheetName);
            return GetCellModelsForSheet(sheet.SheetName).Select(x => CellViewModel.CreateViewModelForModel(x, sheet)).ToList();
        }
    }
}
