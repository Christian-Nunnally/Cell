using Cell.Model;
using Cell.Persistence;
using Cell.Plugin;
using System.Collections.ObjectModel;

namespace Cell.Data
{
    /// <summary>
    /// Contains all cells in the entire application.
    /// </summary>
    internal static class Cells
    {
        private static readonly CellLoader _cellLoader = new(PersistenceManager.SaveLocation);

        private static readonly Dictionary<string, Dictionary<string, CellModel>> _cellsBySheetMap = [];
        private static readonly Dictionary<string, CellModel> _cellsById = [];

        private static readonly Dictionary<string, List<CellModel>> _cellsByLocation = [];
        private static readonly Dictionary<string, string> _cellsToLocation = [];

        public static readonly List<CellModel> _allCells = [];
        public static readonly ReadOnlyCollection<CellModel> AllCells = _allCells.AsReadOnly();

        public static IEnumerable<string> SheetNames => _cellsBySheetMap.Keys;

        public static void AddCell(CellModel cellModel, bool saveAfterAdding = true)
        {
            if (_cellsBySheetMap.TryGetValue(cellModel.SheetName, out var cellDictionary))
            {
                if (cellDictionary.ContainsKey(cellModel.ID)) throw new InvalidOperationException("Cell already added");
                cellDictionary.Add(cellModel.ID, cellModel);
            }
            else
            {
                _cellsBySheetMap.Add(cellModel.SheetName, new Dictionary<string, CellModel> { { cellModel.ID, cellModel } });
            }
            _cellsById.Add(cellModel.ID, cellModel);
            _allCells.Add(cellModel);

            AddCellToCellByLocationMap(cellModel);
            _cellsToLocation.Add(cellModel.ID, cellModel.GetUnqiueLocationString());

            cellModel.PropertyChanged += CellModelPropertyChanged;
            CellTriggerManager.StartMonitoringCell(cellModel);
            CellPopulateManager.StartMonitoringCellForUpdates(cellModel);
            if (saveAfterAdding) _cellLoader.SaveCell(cellModel);
        }

        private static void AddCellToCellByLocationMap(CellModel cellModel)
        {
            if (_cellsByLocation.TryGetValue(cellModel.GetUnqiueLocationString(), out var cellsAtLocation)) cellsAtLocation.Add(cellModel);
            else _cellsByLocation.Add(cellModel.GetUnqiueLocationString(), [cellModel]);
        }

        public static void RemoveCell(CellModel cellModel)
        {
            if (_cellsBySheetMap.TryGetValue(cellModel.SheetName, out var cellDictionary))
            {
                cellDictionary.Remove(cellModel.ID);
                _cellLoader.DeleteCell(cellModel);
                CellTriggerManager.StopMonitoringCell(cellModel);
                CellPopulateManager.StopMonitoringCellForUpdates(cellModel);
                _cellsById.Remove(cellModel.ID);
                _allCells.Remove(cellModel);
                _cellsToLocation.Remove(cellModel.ID);
                _cellsByLocation[cellModel.GetUnqiueLocationString()].Remove(cellModel);
            }
        }

        public static List<CellModel> GetCellModelsForSheet(string sheetName)
        {
            if (_cellsBySheetMap.TryGetValue(sheetName, out var cellDictionary))
            {
                return [.. cellDictionary.Values];
            }
            return [];
        }

        public static CellModel GetCell(string cellId) => _cellsById.TryGetValue(cellId, out var cellModel) ? cellModel : new CellModel();

        public static CellModel? GetCell(string sheet, int row, int column) => _cellsByLocation.TryGetValue(Utilities.GetUnqiueLocationString(sheet, row, column), out var list) ? list.FirstOrDefault() : null;

        private static void CellModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not CellModel model) return;
            if (e.PropertyName == nameof(CellModel.Row) || e.PropertyName == nameof(CellModel.Column) || e.PropertyName == nameof(CellModel.SheetName))
            {
                var previousLocation = _cellsToLocation[model.ID];
                _cellsByLocation[previousLocation].Remove(model);
                _cellsToLocation[model.ID] = model.GetUnqiueLocationString();
                AddCellToCellByLocationMap(model);
            }
            _cellLoader.SaveCell(model);
        }

        public static List<string> GetSheetNames() => [.. _cellsBySheetMap.Keys];
    }
}
