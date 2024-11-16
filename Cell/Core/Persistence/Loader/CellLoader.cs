using Cell.Core.Common;
using Cell.Core.Data.Tracker;
using Cell.Model;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace Cell.Core.Persistence.Loader
{
    /// <summary>
    /// Capable of loading and saving cells to a <see cref="PersistedDirectory"/>.
    /// </summary>
    public class CellLoader
    {
        private readonly CellTracker _cellTracker;
        private readonly PersistedDirectory _sheetsDirectory;
        private bool _isSavingAddedCells = true;
        /// <summary>
        /// Creates a new instance of <see cref="CellLoader"/>.
        /// </summary>
        /// <param name="sheetsDirectory">The project directory to save the cells into.</param>
        /// <param name="cellTracker">Tracker to add loaded cells to.</param>
        public CellLoader(PersistedDirectory sheetsDirectory, CellTracker cellTracker)
        {
            _sheetsDirectory = sheetsDirectory;
            _cellTracker = cellTracker;
            cellTracker.CellAdded += CellAddedToTracker;
            cellTracker.CellRemoved += CellRemovedFromTracker;
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
        public async Task LoadCellsAsync()
        {
            foreach (var sheetDirectory in _sheetsDirectory.GetDirectories().Reverse())
            {
                await LoadSheetAsync(sheetDirectory);
            }
        }

        /// <summary>
        /// Loads all the cells from a specific sheet.
        /// </summary>
        /// <param name="sheet">The name of the sheet to load all cells from.</param>
        /// <returns>All the loaded cells.</returns>
        public async Task LoadSheetAsync(string sheet)
        {
            _isSavingAddedCells = false;
            var sheetDirectory = _sheetsDirectory.FromDirectory(sheet);
            var cornerCell = await sheetDirectory.LoadFileAsync("corner");
            if (!string.IsNullOrWhiteSpace(cornerCell))
            {
                AddSerializedCell(cornerCell);
            }
            foreach (var file in _sheetsDirectory.GetFiles(sheet))
            {
                if (file.EndsWith("corner")) continue;
                await LoadCellAsync(file);
            }
            _isSavingAddedCells = true;
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
            var fileName = cell.ID;
            if (cell.CellType == CellType.Corner)
            {
                // TODO: removed after migration
                //DeleteCell(cell);
                if (!string.IsNullOrEmpty(_sheetsDirectory.LoadFile(Path.Combine(directory, fileName)))) _sheetsDirectory.DeleteFile(Path.Combine(directory, fileName));
                fileName = "corner";
            }
            var serialized = JsonSerializer.Serialize(cell);
            var path = Path.Combine(directory, fileName);
            _sheetsDirectory.SaveFile(path, serialized);
        }

        private void CellAddedToTracker(CellModel cell)
        {
            cell.PropertyChanged += TrackedCellPropertyChanged;
            cell.Location.PropertyChanged += TrackedCellLocationPropertyChanged;
            cell.Style.PropertyChanged += TrackedCellStylePropertyChanged;
            cell.Properties.PropertyChanged += TrackedCellCustomPropertyChanged;

            if (_isSavingAddedCells) SaveCell(cell);
        }

        private void TrackedCellCustomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var properties = (CellModelCustomPropertiesModel)sender!;
            SaveCell(properties.CellModel!);
        }

        private void CellRemovedFromTracker(CellModel cell)
        {
            cell.PropertyChanged -= TrackedCellPropertyChanged;
            cell.Location.PropertyChanged -= TrackedCellLocationPropertyChanged;
            cell.Style.PropertyChanged -= TrackedCellStylePropertyChanged;

            DeleteCell(cell);
        }

        private async Task LoadCellAsync(string file)
        {
            var cellJson = await _sheetsDirectory.LoadFileAsync(file) ?? throw new CellError($"Error loading file {file}");
            AddSerializedCell(cellJson);
        }

        private void AddSerializedCell(string cellJson)
        {
            var cell = JsonSerializer.Deserialize<CellModel>(cellJson) ?? throw new CellError($"Deserialization failed for {cellJson}");
            _cellTracker.AddCell(cell);
        }

        private void TrackedCellLocationPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var style = (CellLocationModel)sender!;
            SaveCell(style.CellModel!);
        }

        private void TrackedCellPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CellModel.PopulateResult)) return;
            var cell = (CellModel)sender!;
            SaveCell(cell);
        }

        private void TrackedCellStylePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var style = (CellStyleModel)sender!;
            SaveCell(style.CellModel!);
        }
    }
}
