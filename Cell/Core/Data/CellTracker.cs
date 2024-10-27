using Cell.Model;
using System.ComponentModel;

namespace Cell.Core.Data
{
    /// <summary>
    /// Contains all cells in the entire application. When a cell is tracked, it is automatically saved to the loader when it changes allows all other parts of the application to access it from this tracker.
    /// </summary>
    public class CellTracker
    {
        private readonly Dictionary<string, List<CellModel>> _cellsByLocation = [];
        private readonly Dictionary<string, Dictionary<string, CellModel>> _cellsBySheetMap = [];
        private readonly Dictionary<CellModel, string> _cellsToLocation = [];
        
        /// <summary>
        /// Occurs when a cell is added to the tracker.
        /// </summary>
        public Action<CellModel>? CellAdded;

        /// <summary>
        /// Occurs when a cell is removed from the tracker.
        /// </summary>
        public Action<CellModel>? CellRemoved;
        
        /// <summary>
        /// Gets all currently tracked cells.
        /// </summary>
        public IEnumerable<CellModel> AllCells => _cellsToLocation.Keys;

        /// <summary>
        /// Adds a new cell to the tracker.
        /// </summary>
        /// <param name="cellModel">The new cell to start tracking.</param>
        public void AddCell(CellModel cellModel)
        {
            AddToCellsInSheetMap(cellModel);
            AddCellToCellByLocationMap(cellModel);
            _cellsToLocation.Add(cellModel, cellModel.Location.LocationString);

            cellModel.Location.PropertyChanged += CellLocationPropertyChanged;
            CellAdded?.Invoke(cellModel);
        }

        /// <summary>
        /// Efficently gets a cell by its location, or null if no cell exists at that location.
        /// </summary>
        /// <param name="cellLocationModel">The location to get the cell from.</param>
        /// <returns>The cell, or null if no cell exists at the given location.</returns>
        public CellModel? GetCell(CellLocationModel cellLocationModel) => GetCell(cellLocationModel.SheetName, cellLocationModel.Row, cellLocationModel.Column);

        /// <summary>
        /// Efficently gets a cell by its location, or null if no cell exists at that location.
        /// </summary>
        /// <param name="sheet">The sheet to look for the cell in.</param>
        /// <param name="row">The row to look for the cell in.</param>
        /// <param name="column">The column to look for the cell in.</param>
        /// <returns>The cell, or null if no cell exists at the given location.</returns>
        public CellModel? GetCell(string sheet, int row, int column) => _cellsByLocation.TryGetValue(new CellLocationModel(sheet, row, column).LocationString, out var list) ? list.FirstOrDefault() : null;

        /// <summary>
        /// Gets all the cells with a given sheet name.
        /// </summary>
        /// <param name="sheetName">The sheet name to get the cells in.</param>
        /// <returns>All the cells in the given sheet.</returns>
        public List<CellModel> GetCellModelsForSheet(string sheetName)
        {
            return _cellsBySheetMap.TryGetValue(sheetName, out var cellDictionary) ? ([.. cellDictionary.Values]) : ([]);
        }

        /// <summary>
        /// Remove a cell from the tracker. This will also inform the loader to delete the cell.
        /// </summary>
        /// <param name="cellModel">The cell to delete.</param>
        public void RemoveCell(CellModel cellModel)
        {
            cellModel.Location.PropertyChanged -= CellLocationPropertyChanged;
            RemoveFromCellsInSheetMap(cellModel, cellModel.Location.SheetName);
            _cellsToLocation.Remove(cellModel);
            RemoveFromCellsByLocationMap(cellModel);
            CellRemoved?.Invoke(cellModel);
        }

        private void AddCellToCellByLocationMap(CellModel cellModel)
        {
            if (_cellsByLocation.TryGetValue(cellModel.Location.LocationString, out var cellsAtLocation))
            {
                cellsAtLocation.Add(cellModel);
            }
            else _cellsByLocation.Add(cellModel.Location.LocationString, [cellModel]);
        }

        private void AddToCellsInSheetMap(CellModel model)
        {
            if (_cellsBySheetMap.TryGetValue(model.Location.SheetName, out var cellMap))
            {
                cellMap.Add(model.ID, model);
            }
            else
            {
                _cellsBySheetMap.Add(model.Location.SheetName, new Dictionary<string, CellModel> { { model.ID, model } });
            }
        }

        private void CellLocationPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var location = (CellLocationModel)sender!;
            var model = location.CellModel!;
            var previousLocation = _cellsToLocation[model];
            _cellsByLocation[previousLocation].Remove(model);
            _cellsToLocation[model] = model.Location.LocationString;
            AddCellToCellByLocationMap(model);
            if (e.PropertyName == nameof(CellModel.Location.SheetName))
            {
                UpdateCellsSheetLocation(model, previousLocation);
            }
        }

        private bool RemoveFromCellsByLocationMap(CellModel cellModel)
        {
            return _cellsByLocation[cellModel.Location.LocationString].Remove(cellModel);
        }

        private bool RemoveFromCellsInSheetMap(CellModel model, string sheetName)
        {
            if (!_cellsBySheetMap.TryGetValue(sheetName, out var cellsInOldSheet)) return false;
            var result = cellsInOldSheet.Remove(model.ID);
            if (cellsInOldSheet.Count == 0)
            {
                _cellsBySheetMap.Remove(sheetName);
            }
            return result;
        }

        private void UpdateCellsSheetLocation(CellModel model, string previousLocation)
        {
            var previousSheetName = previousLocation.Split('_')[0];
            RemoveFromCellsInSheetMap(model, previousSheetName);
            AddToCellsInSheetMap(model);
        }
    }
}
