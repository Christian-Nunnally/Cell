using Cell.Common;
using Cell.Model;
using Cell.Persistence;
using System.ComponentModel;

namespace Cell.Data
{
    /// <summary>
    /// Contains all cells in the entire application. When a cell is tracked, it is automatically saved to the loader when it changes allows all other parts of the application to access it from this tracker.
    /// </summary>
    public class CellTracker
    {
        private readonly CellLoader _cellLoader;
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
        /// Creates a new instance of <see cref="CellTracker"/>.
        /// </summary>
        /// <param name="cellLoader">The cell loader used to provide cells to this tracker. The tracker will also inform the loader to delete cells when needed.</param>
        public CellTracker(CellLoader cellLoader)
        {
            _cellLoader = cellLoader;
        }

        /// <summary>
        /// Gets all currently tracked cells.
        /// </summary>
        public IEnumerable<CellModel> AllCells => _cellsToLocation.Keys;

        /// <summary>
        /// Adds a new cell to the tracker. This will also inform the loader to save the cell if <paramref name="saveAfterAdding"/> is true.
        /// </summary>
        /// <param name="cellModel">The new cell to start tracking.</param>
        /// <param name="saveAfterAdding">Whether to save this cell to the loader.</param>
        public void AddCell(CellModel cellModel, bool saveAfterAdding = true)
        {
            AddToCellsInSheetMap(cellModel);
            AddCellToCellByLocationMap(cellModel);
            _cellsToLocation.Add(cellModel, cellModel.GetUnqiueLocationString());

            cellModel.PropertyChanged += CellModelPropertyChanged;
            CellAdded?.Invoke(cellModel);
            if (saveAfterAdding) _cellLoader.SaveCell(cellModel);
        }

        /// <summary>
        /// Efficently gets a cell by its location, or null if no cell exists at that location.
        /// </summary>
        /// <param name="sheet">The sheet to look for the cell in.</param>
        /// <param name="row">The row to look for the cell in.</param>
        /// <param name="column">The column to look for the cell in.</param>
        /// <returns>The cell, or null if no cell exists at the given location.</returns>
        public CellModel? GetCell(string sheet, int row, int column) => _cellsByLocation.TryGetValue(Utilities.GetUnqiueLocationString(sheet, row, column), out var list) ? list.FirstOrDefault() : null;

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
            cellModel.PropertyChanged -= CellModelPropertyChanged;
            cellModel.Style.PropertyChanged -= CellModelStylePropertyChanged;
            RemoveFromCellsInSheetMap(cellModel, cellModel.SheetName);
            _cellLoader.DeleteCell(cellModel);
            _cellsToLocation.Remove(cellModel);
            RemoveFromCellsByLocationMap(cellModel);
            CellRemoved?.Invoke(cellModel);
        }

        /// <summary>
        /// Renames a sheet by saving the cells in the new sheet and deleting the old sheet.
        /// </summary>
        /// <param name="oldSheetName">The sheet to rename.</param>
        /// <param name="newSheetName">The new name to give the sheet.</param>
        public void RenameSheet(string oldSheetName, string newSheetName)
        {
            _cellLoader.RenameSheet(oldSheetName, newSheetName);
        }

        private void AddCellToCellByLocationMap(CellModel cellModel)
        {
            if (_cellsByLocation.TryGetValue(cellModel.GetUnqiueLocationString(), out var cellsAtLocation))
            {
                cellsAtLocation.Add(cellModel);
            }
            else _cellsByLocation.Add(cellModel.GetUnqiueLocationString(), [cellModel]);
        }

        private void AddToCellsInSheetMap(CellModel model)
        {
            if (_cellsBySheetMap.TryGetValue(model.SheetName, out var cellMap))
            {
                cellMap.Add(model.ID, model);
            }
            else
            {
                _cellsBySheetMap.Add(model.SheetName, new Dictionary<string, CellModel> { { model.ID, model } });
            }
        }

        private void CellModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not CellModel model) return;
            if (e.PropertyName == nameof(CellModel.Row) || e.PropertyName == nameof(CellModel.Column) || e.PropertyName == nameof(CellModel.SheetName))
            {
                var previousLocation = _cellsToLocation[model];
                _cellsByLocation[previousLocation].Remove(model);
                _cellsToLocation[model] = model.GetUnqiueLocationString();
                AddCellToCellByLocationMap(model);
                if (e.PropertyName == nameof(CellModel.SheetName))
                {
                    UpdateCellsSheetLocation(model, previousLocation);
                }
            }
            _cellLoader.SaveCell(model);
        }

        private void UpdateCellsSheetLocation(CellModel model, string previousLocation)
        {
            var previousSheetName = previousLocation.Split('_')[0];
            RemoveFromCellsInSheetMap(model, previousSheetName);
            AddToCellsInSheetMap(model);
        }

        private void CellModelStylePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not CellModel model) return;
            _cellLoader.SaveCell(model);
        }

        private bool RemoveFromCellsByLocationMap(CellModel cellModel)
        {
            return _cellsByLocation[cellModel.GetUnqiueLocationString()].Remove(cellModel);
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
    }
}
