using Cell.Common;
using Cell.Model;
using Cell.Persistence;
using System.ComponentModel;

namespace Cell.Data
{
    /// <summary>
    /// Contains all cells in the entire application.
    /// </summary>
    public class CellTracker
    {
        private readonly CellLoader _cellLoader;
        private readonly Dictionary<string, List<CellModel>> _cellsByLocation = [];
        private readonly Dictionary<string, Dictionary<string, CellModel>> _cellsBySheetMap = [];
        private readonly Dictionary<string, string> _cellsToLocation = [];

        public Action<CellModel>? CellAdded;
        public Action<CellModel>? CellRemoved;

        public CellTracker(CellLoader cellLoader)
        {
            _cellLoader = cellLoader;
        }

        public IEnumerable<CellModel> AllCells => _cellsBySheetMap.Values.SelectMany(x => x.Values);

        public void AddCell(CellModel cellModel, bool saveAfterAdding = true)
        {
            AddToCellsInSheetMap(cellModel);
            AddCellToCellByLocationMap(cellModel);
            _cellsToLocation.Add(cellModel.ID, cellModel.GetUnqiueLocationString());

            cellModel.PropertyChanged += CellModelPropertyChanged;
            cellModel.StylePropertyChanged += CellModelStylePropertyChanged;
            CellAdded?.Invoke(cellModel);
            if (saveAfterAdding) _cellLoader.SaveCell(cellModel);
        }

        private void CellModelStylePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not CellModel model) return;
            _cellLoader.SaveCell(model);
        }

        public CellModel? GetCell(string sheet, int row, int column) => _cellsByLocation.TryGetValue(Utilities.GetUnqiueLocationString(sheet, row, column), out var list) ? list.FirstOrDefault() : null;

        public List<CellModel> GetCellModelsForSheet(string sheetName)
        {
            if (_cellsBySheetMap.TryGetValue(sheetName, out var cellDictionary))
            {
                return [.. cellDictionary.Values];
            }
            return [];
        }

        public void RemoveCell(CellModel cellModel)
        {
            cellModel.PropertyChanged -= CellModelPropertyChanged;
            cellModel.Style.PropertyChanged -= CellModelStylePropertyChanged;
            RemoveFromCellsInSheetMap(cellModel, cellModel.SheetName);
            _cellLoader.DeleteCell(cellModel);
            _cellsToLocation.Remove(cellModel.ID);
            RemoveFromCellsByLocationMap(cellModel);
            CellRemoved?.Invoke(cellModel);
        }

        internal void RenameSheet(string oldSheetName, string newSheetName)
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

        private void CellModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not CellModel model) return;
            if (e.PropertyName == nameof(CellModel.Row) || e.PropertyName == nameof(CellModel.Column) || e.PropertyName == nameof(CellModel.SheetName))
            {
                var previousLocation = _cellsToLocation[model.ID];
                _cellsByLocation[previousLocation].Remove(model);
                _cellsToLocation[model.ID] = model.GetUnqiueLocationString();
                AddCellToCellByLocationMap(model);

                if (e.PropertyName == nameof(CellModel.SheetName))
                {
                    // Remove
                    var previousSheetName = previousLocation.Split('_')[0];
                    RemoveFromCellsInSheetMap(model, previousSheetName);

                    // Add
                    AddToCellsInSheetMap(model);
                }
            }
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

        public void SaveSheet(string sheetName)
        {
            GetCellModelsForSheet(sheetName).ForEach(_cellLoader.SaveCell);
        }
    }
}
