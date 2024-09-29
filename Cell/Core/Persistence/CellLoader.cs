using Cell.Common;
using Cell.Model;
using System.IO;
using System.Text.Json;

namespace Cell.Persistence
{
    public class CellLoader
    {
        private const string SheetsSaveDirectory = "Sheets";
        private readonly PersistedDirectory _persistenceManager;

        private event Action<CellModel> CellLoaded;

        public CellLoader(PersistedDirectory persistenceManager)
        {
            _persistenceManager = persistenceManager;
        }

        public void DeleteCell(CellModel cellModel)
        {
            var cellDirectory = Path.Combine(SheetsSaveDirectory, cellModel.SheetName);
            var cellPath = Path.Combine(cellDirectory, cellModel.ID);
            _persistenceManager.DeleteFile(cellPath);
            if (!_persistenceManager.GetFiles(cellDirectory).Any()) _persistenceManager.DeleteDirectory(cellDirectory);
        }

        public CellModel LoadCell(string file)
        {
            var text = _persistenceManager.LoadFile(file) ?? throw new CellError($"Error loading file {file}");
            var cell = JsonSerializer.Deserialize<CellModel>(text) ?? throw new CellError($"Deserialization failed for {text} at {file}");
            CellLoaded?.Invoke(cell);
            return cell;
        }

        public IEnumerable<CellModel> LoadCells()
        {
            if (!_persistenceManager.DirectoryExists(SheetsSaveDirectory)) return [];

            var cells = new List<CellModel>();
            foreach (var sheetDirectory in _persistenceManager.GetDirectories(SheetsSaveDirectory))
            {
                cells.AddRange(LoadSheet(sheetDirectory));
            }
            return cells;
        }

        public IEnumerable<CellModel> LoadSheet(string directory)
        {
            var result = new List<CellModel>();
            foreach (var file in _persistenceManager.GetFiles(directory)) result.Add(LoadCell(file));
            return result;
        }

        public void RenameSheet(string oldName, string newName)
        {
            var oldDirectory = Path.Combine(SheetsSaveDirectory, oldName);
            var newDirectory = Path.Combine(SheetsSaveDirectory, newName);
            _persistenceManager.MoveDirectory(oldDirectory, newDirectory);
        }

        public void SaveCell(CellModel cell)
        {
            var directory = Path.Combine(SheetsSaveDirectory, cell.SheetName);
            SaveCell(directory, cell);
        }

        public void SaveCell(string directory, CellModel cell)
        {
            var serialized = JsonSerializer.Serialize(cell);
            var path = Path.Combine(directory, cell.ID);
            _persistenceManager.SaveFile(path, serialized);
        }
    }
}
