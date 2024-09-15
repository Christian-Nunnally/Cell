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

        public IEnumerable<CellModel> LoadCells()
        {
            if (!_persistenceManager.DirectoryExists(SheetsSaveDirectory)) return [];

            var cells = new List<CellModel>();
            foreach (var directory in _persistenceManager.GetDirectories(SheetsSaveDirectory))
            {
                foreach (var file in _persistenceManager.GetFiles(directory)) cells.Add(LoadCell(file));
            }
            return cells;
        }

        public CellModel LoadCell(string file)
        {
            var text = _persistenceManager.LoadFile(file) ?? throw new CellError($"Error loading file {file}");
            var cell = JsonSerializer.Deserialize<CellModel>(text) ?? throw new CellError($"Deserialization failed for {text} at {file}");
            return cell;
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

        private void FixMergedCellsWithNewIdentities(IEnumerable<CellModel> cells, Dictionary<string, string> oldIdToNewIdMap)
        {
            foreach (var cell in cells.Where(cell => cell.MergedWith != string.Empty))
            {
                cell.MergedWith = oldIdToNewIdMap[cell.MergedWith];
            }
        }

        private Dictionary<string, string> GiveCellsNewUniqueIndentities(string sheetName, IEnumerable<CellModel> cells)
        {
            var oldIdToNewIdMap = new Dictionary<string, string>();
            foreach (var cell in cells)
            {
                var newId = Guid.NewGuid().ToString();
                oldIdToNewIdMap[cell.ID] = newId;
                cell.ID = newId;
                cell.SheetName = sheetName;
            }
            return oldIdToNewIdMap;
        }

        public void UpdateIdentitiesOfCellsForNewSheet(string sheetName, IEnumerable<CellModel> cellsToAdd)
        {
            var oldIdToNewIdMap = GiveCellsNewUniqueIndentities(sheetName, cellsToAdd);
            FixMergedCellsWithNewIdentities(cellsToAdd, oldIdToNewIdMap);
        }
    }
}
