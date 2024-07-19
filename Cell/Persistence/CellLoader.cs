using Cell.Exceptions;
using Cell.Model;
using System.IO;

namespace Cell.Persistence
{
    static class CellLoader
    {
        public static void LoadCells()
        {
            var sheetsPath = Path.Combine(PersistenceManager.SaveLocation, "Sheets");
            if (Directory.Exists(sheetsPath))
            {
                foreach (var directory in Directory.GetDirectories(sheetsPath))
                {
                    LoadSheet(directory);
                }
            }
            foreach (var cell in Cells.AllCells)
            {
                if (cell.NeedsUpdateDependencySubscriptionsToBeCalled)
                {
                    if (!cell.UpdateDependencySubscriptions()) throw new ProjectLoadException($"Unable to update dependency subscriptions for {cell.ID} even after all cells have been loaded.");
                }
            }
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
            Cells.AddCell(cell);
            return cell;
        }

        public static void DeleteCell(CellModel cellModel)
        {
            var directory = Path.Combine(PersistenceManager.SaveLocation, "Sheets", cellModel.SheetName);
            if (!Directory.Exists(directory)) return;
            File.Delete(Path.Combine(directory, cellModel.ID));
        }

        public static void SaveCells()
        {
            foreach (var sheet in Cells.SheetNames)
            {
                SaveSheet(sheet);
            }
        }

        private static void SaveSheet(string sheet)
        {
            foreach (var cell in Cells.GetCellModelsForSheet(sheet))
            {
                SaveCell(cell);
            }
        }

        public static void SaveCell(CellModel cell)
        {
            var directory = Path.Combine(PersistenceManager.SaveLocation, "Sheets", cell.SheetName);
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, cell.ID), CellModel.SerializeModel(cell));
        }
    }
}
