using Cell.Data;
using Cell.Exceptions;
using Cell.Model;
using System.IO;

namespace Cell.Persistence
{
    public class CellLoader(string saveDirectory)
    {
        private const string SheetsSaveDirectory = "Sheets";
        private readonly string _saveDirectory = saveDirectory;

        public void LoadCells()
        {
            LoadCellsInternal();
            ComputeDependenciesThatDependOnCellsToBeLoaded();
        }

        private void LoadCellsInternal()
        {
            var sheetsPath = Path.Combine(_saveDirectory, SheetsSaveDirectory);
            if (!Directory.Exists(sheetsPath)) return;
            foreach (var directory in Directory.GetDirectories(sheetsPath)) LoadSheet(directory);
        }

        private static void ComputeDependenciesThatDependOnCellsToBeLoaded()
        {
            foreach (var cell in Cells.AllCells.Where(x => x.NeedsUpdateDependencySubscriptionsToBeCalled))
            {
                if (!cell.UpdateDependencySubscriptions()) throw new ProjectLoadException($"Unable to update dependency subscriptions for {cell.ID} even after all cells have been loaded.");
            }
        }

        private static void LoadSheet(string directory)
        {
            foreach (var file in Directory.GetFiles(directory)) LoadCell(file);
        }

        public void SaveCells()
        {
            foreach (var sheet in Cells.SheetNames) SaveSheet(sheet);
        }

        private void SaveSheet(string sheet)
        {
            foreach (var cell in Cells.GetCellModelsForSheet(sheet)) SaveCell(cell);
        }

        public void SaveCell(CellModel cell)
        {
            var directory = Path.Combine(_saveDirectory, SheetsSaveDirectory, cell.SheetName);
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, cell.ID), CellModel.SerializeModel(cell));
        }

        private static CellModel LoadCell(string file)
        {
            var cell = CellModel.DeserializeModel(File.ReadAllText(file));
            Cells.AddCell(cell, saveAfterAdding: false);
            return cell;
        }

        public void DeleteCell(CellModel cellModel)
        {
            var directory = Path.Combine(_saveDirectory, SheetsSaveDirectory, cellModel.SheetName);
            if (!Directory.Exists(directory)) return;
            File.Delete(Path.Combine(directory, cellModel.ID));
        }
    }
}
