using Cell.Data;
using Cell.Exceptions;
using Cell.Model;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    public class CellLoader(string saveDirectory)
    {
        private const string SheetsSaveDirectory = "Sheets";
        private readonly string _saveDirectory = saveDirectory;

        public void LoadCells()
        {
            var sheetsPath = Path.Combine(_saveDirectory, SheetsSaveDirectory);
            if (!Directory.Exists(sheetsPath)) return;
            foreach (var directory in Directory.GetDirectories(sheetsPath)) LoadSheet(directory);
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

        public void RenameSheet(string oldName, string newName)
        {
            var oldDirectory = Path.Combine(_saveDirectory, SheetsSaveDirectory, oldName);
            var newDirectory = Path.Combine(_saveDirectory, SheetsSaveDirectory, newName);
            Directory.Move(oldDirectory, newDirectory);
        }

        public void SaveCell(CellModel cell)
        {
            var directory = Path.Combine(_saveDirectory, SheetsSaveDirectory, cell.SheetName);
            Directory.CreateDirectory(directory);
            var serialized = JsonSerializer.Serialize(cell);
            var path = Path.Combine(directory, cell.ID);
            File.WriteAllText(path, serialized);
        }

        private static CellModel LoadCell(string file)
        {
            var text = File.ReadAllText(file) ?? throw new ProjectLoadException($"Loading file failed at {file}"); ;
            var cell = JsonSerializer.Deserialize<CellModel>(text) ?? throw new ProjectLoadException($"Deserialization failed for {File.ReadAllText(file)} at {file}");
            Cells.AddCell(cell, saveAfterAdding: false);
            return cell;
        }

        public void DeleteCell(CellModel cellModel)
        {
            var directory = Path.Combine(_saveDirectory, SheetsSaveDirectory, cellModel.SheetName);
            if (!Directory.Exists(directory)) return;
            var path = Path.Combine(directory, cellModel.ID);
            File.Delete(path);
        }
    }
}
