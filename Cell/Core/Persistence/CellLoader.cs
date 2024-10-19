using Cell.Core.Common;
using Cell.Model;
using System.IO;
using System.Text.Json;

namespace Cell.Core.Persistence
{
    /// <summary>
    /// Capable of loading and saving cells to a <see cref="PersistedDirectory"/>.
    /// </summary>
    public class CellLoader
    {
        private readonly PersistedDirectory _sheetsDirectory;
        /// <summary>
        /// Creates a new instance of <see cref="CellLoader"/>.
        /// </summary>
        /// <param name="sheetsDirectory">The project directory to save the cells into.</param>
        public CellLoader(PersistedDirectory sheetsDirectory)
        {
            _sheetsDirectory = sheetsDirectory;
        }

        /// <summary>
        /// Deletes a cell from the project.
        /// </summary>
        /// <param name="cellModel"></param>
        public void DeleteCell(CellModel cellModel)
        {
            var cellDirectory = cellModel.Location.SheetName;
            var cellPath = Path.Combine(cellModel.Location.SheetName, cellModel.ID);
            _sheetsDirectory.DeleteFile(cellPath);
            if (!_sheetsDirectory.GetFiles(cellDirectory).Any()) _sheetsDirectory.DeleteDirectory(cellDirectory);
        }

        /// <summary>
        /// Loads all cells from the project.
        /// </summary>
        /// <returns>All loaded cells.</returns>
        public IEnumerable<CellModel> LoadCells()
        {
            var cells = new List<CellModel>();
            foreach (var sheetDirectory in _sheetsDirectory.GetDirectories())
            {
                cells.AddRange(LoadSheet(sheetDirectory));
            }
            return cells;
        }

        /// <summary>
        /// Loads all the cells from a specific sheet.
        /// </summary>
        /// <param name="sheet">The name of the sheet to load all cells from.</param>
        /// <returns>All the loaded cells.</returns>
        public IEnumerable<CellModel> LoadSheet(string sheet)
        {
            var result = new List<CellModel>();
            foreach (var file in _sheetsDirectory.GetFiles(sheet)) result.Add(LoadCell(file));
            return result;
        }

        /// <summary>
        /// Moves the cells from one sheet to another to facilitate renaming a sheet.
        /// </summary>
        /// <param name="oldName">The old sheet name.</param>
        /// <param name="newName">The new sheet name.</param>
        public void RenameSheet(string oldName, string newName)
        {
            _sheetsDirectory.MoveDirectory(oldName, newName);
        }

        /// <summary>
        /// Saves a cell to the project.
        /// </summary>
        /// <param name="cell">The cell to save.</param>
        public void SaveCell(CellModel cell)
        {
            SaveCell(cell.Location.SheetName, cell);
        }

        /// <summary>
        /// Saves a cell to a specific path within the project.
        /// </summary>
        /// <param name="directory">The path inside the project to save the cell.</param>
        /// <param name="cell">The cell to save.</param>
        public void SaveCell(string directory, CellModel cell)
        {
            var serialized = JsonSerializer.Serialize(cell);
            var path = Path.Combine(directory, cell.ID);
            _sheetsDirectory.SaveFile(path, serialized);
        }

        private CellModel LoadCell(string file)
        {
            var text = _sheetsDirectory.LoadFile(file) ?? throw new CellError($"Error loading file {file}");
            var cell = JsonSerializer.Deserialize<CellModel>(text) ?? throw new CellError($"Deserialization failed for {text} at {file}");
            return cell;
        }
    }
}
