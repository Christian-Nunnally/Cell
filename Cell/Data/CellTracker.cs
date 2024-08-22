using Cell.Common;
using Cell.Execution;
using Cell.Model;
using Cell.Persistence;
using System.Collections.ObjectModel;

namespace Cell.Data
{
    /// <summary>
    /// Contains all cells in the entire application.
    /// </summary>
    internal class CellTracker
    {
        private readonly CellLoader _cellLoader = new(PersistenceManager.CurrentRootPath);
        private readonly Dictionary<string, List<CellModel>> _cellsByLocation = [];
        private readonly Dictionary<string, Dictionary<string, CellModel>> _cellsBySheetMap = [];
        private readonly Dictionary<string, string> _cellsToLocation = [];
        private readonly ObservableCollection<string> _sheetNames = [];
        private static CellTracker? _instance;
        public static CellTracker Instance => _instance ??= new CellTracker();

        public IEnumerable<CellModel> AllCells => _cellsBySheetMap.Values.SelectMany(x => x.Values);

        public ObservableCollection<string> SheetNames => _sheetNames;

        public void AddCell(CellModel cellModel, bool saveAfterAdding = true)
        {
            AddToCellsInSheetMap(cellModel);
            AddCellToCellByLocationMap(cellModel);
            _cellsToLocation.Add(cellModel.ID, cellModel.GetUnqiueLocationString());

            cellModel.PropertyChanged += CellModelPropertyChanged;
            CellTriggerManager.StartMonitoringCell(cellModel);
            CellPopulateManager.StartMonitoringCellForUpdates(cellModel);
            if (saveAfterAdding) _cellLoader.SaveCell(cellModel);
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
            _cellLoader.DeleteCell(cellModel);
            CellTriggerManager.StopMonitoringCell(cellModel);
            CellPopulateManager.StopMonitoringCellForUpdates(cellModel);
            CellPopulateManager.UnsubscribeFromAllLocationUpdates(cellModel);
            CellPopulateManager.UnsubscribeFromAllCollectionUpdates(cellModel);
            _cellsToLocation.Remove(cellModel.ID);
            RemoveFromCellsByLocationMap(cellModel);
        }

        private bool RemoveFromCellsByLocationMap(CellModel cellModel)
        {
            return _cellsByLocation[cellModel.GetUnqiueLocationString()].Remove(cellModel);
        }

        public void RenameSheet(string oldSheetName, string newSheetName)
        {
            _cellLoader.RenameSheet(oldSheetName, newSheetName);
            GetCellModelsForSheet(oldSheetName).ForEach(x => x.SheetName = newSheetName);
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
            _cellLoader.SaveCell(model);
        }

        private void AddToCellsInSheetMap(CellModel model)
        {
            if (_cellsBySheetMap.TryGetValue(model.SheetName, out var cellsInNewSheet))
            {
                cellsInNewSheet.Add(model.ID, model);
            }
            else
            {
                _cellsBySheetMap.Add(model.SheetName, new Dictionary<string, CellModel> { { model.ID, model } });
                _sheetNames.Add(model.SheetName);
            }
        }

        private bool RemoveFromCellsInSheetMap(CellModel model, string sheet)
        {
            if (!_cellsBySheetMap.TryGetValue(sheet, out var cellsInOldSheet)) return false;
            var result = cellsInOldSheet.Remove(model.ID);
            if (cellsInOldSheet.Count == 0)
            {
                _cellsBySheetMap.Remove(sheet);
                _sheetNames.Remove(sheet);
            }
            return result;
        }
    }
}
