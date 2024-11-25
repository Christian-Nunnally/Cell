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
        /// <summary>
        /// Occurs when all the sheets have finished loading. What is not the same as all cells being loaded.
        /// </summary>
        public Action? SheetsLoaded;
        private bool _isSavingAddedCells = true;
        private readonly List<PersistedDirectory> _partiallyLoadedSheets = [];
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
            if (cellModel.CellType == CellType.Corner)
            {
                cellPath = Path.Combine(cellModel.Location.SheetName, "Corner", cellModel.ID);
            }
            if (cellModel.CellType == CellType.Row)
            {
                cellPath = Path.Combine(cellModel.Location.SheetName, "Rows", cellModel.ID);
            }
            if (cellModel.CellType == CellType.Column)
            {
                cellPath = Path.Combine(cellModel.Location.SheetName, "Columns", cellModel.ID);
            }
            _sheetsDirectory.DeleteFile(cellPath);
            if (!_sheetsDirectory.GetFiles(cellDirectory).Any()) _sheetsDirectory.DeleteDirectory(cellDirectory);
        }

        /// <summary>
        /// Loads all cells from the project.
        /// </summary>
        /// <returns>All loaded cells.</returns>
        public async Task FinishLoadingSheetsAsync()
        {
            _isSavingAddedCells = false;
            foreach (var sheetDirectory in _partiallyLoadedSheets)
            {
                var rows = sheetDirectory.FromDirectory("Rows");
                var columns = sheetDirectory.FromDirectory("Columns");
                foreach (var file in rows.GetFiles(""))
                {
                    var cellJson = await rows.LoadFileAsync(file) ?? throw new CellError($"Error loading file {file}");
                    AddSerializedCell(cellJson);
                }
                foreach (var file in columns.GetFiles(""))
                {
                    var cellJson = await columns.LoadFileAsync(file) ?? throw new CellError($"Error loading file {file}");
                    AddSerializedCell(cellJson);
                }
                foreach (var file in sheetDirectory.GetFiles(""))
                {
                    var cellJson = await sheetDirectory.LoadFileAsync(file) ?? throw new CellError($"Error loading file {file}");
                    AddSerializedCell(cellJson);
                }
            }
            _partiallyLoadedSheets.Clear();
            _isSavingAddedCells = true;
        }

        /// <summary>
        /// Loads the corner cell of all sheets.
        /// </summary>
        /// <returns>All loaded cells.</returns>
        public async Task LoadSheetsAsync()
        {
            foreach (var sheetDirectory in _sheetsDirectory.GetDirectories())
            {
                _isSavingAddedCells = false;
                var persistedSheetDirectory = _sheetsDirectory.FromDirectory(sheetDirectory);
                var cornerDirectory = persistedSheetDirectory.FromDirectory("Corner");
                var cornerCell = await cornerDirectory.LoadFileAsync("corner");
                if (!string.IsNullOrWhiteSpace(cornerCell))
                {
                    AddSerializedCell(cornerCell);
                    _partiallyLoadedSheets.Add(persistedSheetDirectory);
                }
                else
                {
                    throw new NotImplementedException("Load when no 'corner' cell is found");
                }
                _isSavingAddedCells = true;
            }
            SheetsLoaded?.Invoke();
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
            var serialized = JsonSerializer.Serialize(cell);
            var path = Path.Combine(directory, fileName);
            if (cell.CellType == CellType.Corner)
            {
                fileName = "corner";
                // TODO: removed after migration
                if (!string.IsNullOrEmpty(_sheetsDirectory.LoadFile(Path.Combine(directory, fileName)))) _sheetsDirectory.DeleteFile(Path.Combine(directory, fileName));

                path = Path.Combine(directory, "Corner", fileName);
            }
            if (cell.CellType == CellType.Row)
            {
                // TODO: removed after migration
                if (!string.IsNullOrEmpty(_sheetsDirectory.LoadFile(Path.Combine(directory, fileName)))) _sheetsDirectory.DeleteFile(Path.Combine(directory, fileName));

                path = Path.Combine(directory, "Rows", fileName);
            }
            if (cell.CellType == CellType.Column)
            {
                // TODO: removed after migration
                if (!string.IsNullOrEmpty(_sheetsDirectory.LoadFile(Path.Combine(directory, fileName)))) _sheetsDirectory.DeleteFile(Path.Combine(directory, fileName));

                path = Path.Combine(directory, "Columns", fileName);
            }
            _sheetsDirectory.SaveFile(path, serialized);
        }

        private void AddSerializedCell(string cellJson)
        {
            var cell = JsonSerializer.Deserialize<CellModel>(cellJson) ?? throw new CellError($"Deserialization failed for {cellJson}");
            _cellTracker.AddCell(cell);
        }

        private void CellAddedToTracker(CellModel cell)
        {
            cell.PropertyChanged += TrackedCellPropertyChanged;
            cell.Location.PropertyChanged += TrackedCellLocationPropertyChanged;
            cell.Style.PropertyChanged += TrackedCellStylePropertyChanged;
            cell.Properties.PropertyChanged += TrackedCellCustomPropertyChanged;

            if (_isSavingAddedCells) SaveCell(cell);
        }

        private void CellRemovedFromTracker(CellModel cell)
        {
            cell.PropertyChanged -= TrackedCellPropertyChanged;
            cell.Location.PropertyChanged -= TrackedCellLocationPropertyChanged;
            cell.Style.PropertyChanged -= TrackedCellStylePropertyChanged;

            DeleteCell(cell);
        }

        private void TrackedCellCustomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var properties = (CellModelCustomPropertiesModel)sender!;
            SaveCell(properties.CellModel!);
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
