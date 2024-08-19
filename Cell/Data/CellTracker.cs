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
            if (_cellsBySheetMap.TryGetValue(cellModel.SheetName, out var cellDictionary))
            {
                if (cellDictionary.ContainsKey(cellModel.ID)) throw new InvalidOperationException("Cell already added");
                cellDictionary.Add(cellModel.ID, cellModel);
            }
            else
            {
                _cellsBySheetMap.Add(cellModel.SheetName, new Dictionary<string, CellModel> { { cellModel.ID, cellModel } });
                _sheetNames.Add(cellModel.SheetName);
            }

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
            if (!_cellsBySheetMap.TryGetValue(cellModel.SheetName, out var cellDictionary)) return;
            cellDictionary.Remove(cellModel.ID);
            _cellLoader.DeleteCell(cellModel);
            CellTriggerManager.StopMonitoringCell(cellModel);
            CellPopulateManager.StopMonitoringCellForUpdates(cellModel);
            CellPopulateManager.UnsubscribeFromAllLocationUpdates(cellModel);
            CellPopulateManager.UnsubscribeFromAllCollectionUpdates(cellModel);
            _cellsToLocation.Remove(cellModel.ID);
            _cellsByLocation[cellModel.GetUnqiueLocationString()].Remove(cellModel);
            if (cellDictionary.Count == 0)
            {
                _cellsBySheetMap.Remove(cellModel.SheetName);
                _sheetNames.Remove(cellModel.SheetName);
            }
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
                    if (_cellsBySheetMap.TryGetValue(previousSheetName, out var cellsInOldSheet))
                    {
                        cellsInOldSheet.Remove(model.ID);
                        if (cellsInOldSheet.Count == 0)
                        {
                            _cellsBySheetMap.Remove(previousSheetName);
                            _sheetNames.Remove(previousSheetName);
                        }
                    }

                    // Add
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
            }
            _cellLoader.SaveCell(model);
        }
    }
}
