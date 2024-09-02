using Cell.Common;
using Cell.Execution;
using Cell.Model;
using Cell.ViewModel.Application;

namespace Cell.Data
{
    /// <summary>
    /// Contains all cells in the entire application.
    /// </summary>
    public class CellTracker
    {
        private readonly Dictionary<string, List<CellModel>> _cellsByLocation = [];
        private readonly Dictionary<string, Dictionary<string, CellModel>> _cellsBySheetMap = [];
        private readonly Dictionary<string, string> _cellsToLocation = [];
        private readonly CellTriggerManager _trigerManager;
        private readonly CellPopulateManager _populateManager;
        private readonly SheetTracker _sheetTracker;

        public IEnumerable<CellModel> AllCells => _cellsBySheetMap.Values.SelectMany(x => x.Values);

        public CellTracker(SheetTracker sheetTracker, CellTriggerManager trigerManager, CellPopulateManager populateManager)
        {
            _trigerManager = trigerManager;
            _populateManager = populateManager;
            _sheetTracker = sheetTracker;
        }

        public void AddCell(CellModel cellModel, bool saveAfterAdding = true)
        {
            AddToCellsInSheetMap(cellModel);
            AddCellToCellByLocationMap(cellModel);
            _cellsToLocation.Add(cellModel.ID, cellModel.GetUnqiueLocationString());

            cellModel.PropertyChanged += CellModelPropertyChanged;
            _trigerManager.StartMonitoringCell(cellModel);
            _populateManager.StartMonitoringCellForUpdates(cellModel);
            if (saveAfterAdding) ApplicationViewModel.Instance.CellLoader.SaveCell(cellModel);
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
            RemoveFromCellsInSheetMap(cellModel, cellModel.SheetName);
            ApplicationViewModel.Instance.CellLoader.DeleteCell(cellModel);
            _trigerManager.StopMonitoringCell(cellModel);
            _populateManager.StopMonitoringCellForUpdates(cellModel);
            _populateManager.UnsubscribeFromAllLocationUpdates(cellModel);
            _populateManager.UnsubscribeFromAllCollectionUpdates(cellModel);
            _cellsToLocation.Remove(cellModel.ID);
            RemoveFromCellsByLocationMap(cellModel);
        }

        private bool RemoveFromCellsByLocationMap(CellModel cellModel)
        {
            return _cellsByLocation[cellModel.GetUnqiueLocationString()].Remove(cellModel);
        }

        private void AddCellToCellByLocationMap(CellModel cellModel)
        {
            if (_cellsByLocation.TryGetValue(cellModel.GetUnqiueLocationString(), out var cellsAtLocation))
            {
                cellsAtLocation.Add(cellModel);
            }
            else _cellsByLocation.Add(cellModel.GetUnqiueLocationString(), [cellModel]);
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
            ApplicationViewModel.Instance.CellLoader.SaveCell(model);
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
                var sheet = _sheetTracker.Sheets.FirstOrDefault(x => x.Name == model.SheetName);
                if (sheet == null)
                { 
                    sheet = new SheetModel(model.SheetName);
                    _sheetTracker.Sheets.Add(sheet);
                }
            }

            if (model.CellType == CellType.Corner)
            {
                _sheetTracker.Sheets.First(x => x.Name == model.SheetName).CornerCell = model;
            }
        }

        private bool RemoveFromCellsInSheetMap(CellModel model, string sheetName)
        {
            if (!_cellsBySheetMap.TryGetValue(sheetName, out var cellsInOldSheet)) return false;
            var result = cellsInOldSheet.Remove(model.ID);
            if (cellsInOldSheet.Count == 0)
            {
                _cellsBySheetMap.Remove(sheetName);
                var sheet = _sheetTracker.Sheets.First(x => x.OldName == sheetName);
                _sheetTracker.Sheets.Remove(sheet);
                // TODO: Delete sheet from disk, and handle closing the sheet if it is open. Actually mabye just don't allow deleting the open sheet
            }
            return result;
        }
    }
}
