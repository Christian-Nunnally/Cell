using Cell.Model;
using Cell.Plugin;
using Cell.ViewModel;
using System.Collections.ObjectModel;

namespace Cell.Persistence
{
    /// <summary>
    /// Contains all cells in the entire application.
    /// </summary>
    internal static class Cells
    {
        private readonly static Dictionary<string, Dictionary<string, CellModel>> _cellsBySheetMap = [];
        private readonly static Dictionary<string, CellModel> _cellsById = [];
        private readonly static Dictionary<int, List<CellModel>> _rowCellsByIndex = [];
        private readonly static Dictionary<int, List<CellModel>> _columnCellsByIndex = [];
        private readonly static Dictionary<CellModel, int> _rowModelToRowIndexMap = [];
        private readonly static Dictionary<CellModel, int> _columnModelToColumnIndexMap = [];

        public static readonly List<CellModel> _allCells = [];
        public static readonly ReadOnlyCollection<CellModel> AllCells = _allCells.AsReadOnly();

        public static IEnumerable<string> SheetNames => _cellsBySheetMap.Keys;

        public static void AddCell(CellModel cellModel)
        {
            if (_cellsBySheetMap.TryGetValue(cellModel.SheetName, out var cellDictionary))
            {
                if (cellDictionary.ContainsKey(cellModel.ID))
                {
                    throw new InvalidOperationException("Cell already added");
                }
                cellDictionary.Add(cellModel.ID, cellModel);
            }
            else
            {
                _cellsBySheetMap.Add(cellModel.SheetName, new Dictionary<string, CellModel> { { cellModel.ID, cellModel } });
            }
            _cellsById.Add(cellModel.ID, cellModel);
            _allCells.Add(cellModel);

            if (cellModel.Row == 0)
            {
                _columnModelToColumnIndexMap[cellModel] = cellModel.Column;
                AddCellToColumnCellsByIndexMap(cellModel);
            }
            if (cellModel.Column == 0)
            {
                _rowModelToRowIndexMap[cellModel] = cellModel.Row;
                AddCellToRowCellsByIndexMap(cellModel);
            }

            cellModel.PropertyChanged += CellModelPropertyChanged;
            CellEditManager.StartMonitoringCellForEdits(cellModel);
            CellUpdateManager.StartMonitoringCellForUpdates(cellModel);
        }

        private static void AddCellToColumnCellsByIndexMap(CellModel cellModel)
        {
            if (_columnCellsByIndex.TryGetValue(cellModel.Column, out var columnCells)) columnCells.Add(cellModel);
            else _columnCellsByIndex.Add(cellModel.Column, [cellModel]);
        }

        private static void AddCellToRowCellsByIndexMap(CellModel cellModel)
        {
            if (_rowCellsByIndex.TryGetValue(cellModel.Row, out var rowCells)) rowCells.Add(cellModel);
            else _rowCellsByIndex.Add(cellModel.Row, [cellModel]);
        }

        public static void RemoveCell(CellModel cellModel)
        {
            if (_cellsBySheetMap.TryGetValue(cellModel.SheetName, out var cellDictionary))
            {
                cellDictionary.Remove(cellModel.ID);
                CellLoader.DeleteCell(cellModel);
                CellEditManager.StopMonitoringCellForEdits(cellModel);
                CellUpdateManager.StopMonitoringCellForUpdates(cellModel);
                _cellsById.Remove(cellModel.ID);
                _allCells.Remove(cellModel);
                if (cellModel.Column == 0)
                {
                    _rowModelToRowIndexMap.Remove(cellModel);
                    _rowCellsByIndex[cellModel.Row].Remove(cellModel);
                }
                if (cellModel.Row == 0)
                {
                    _columnModelToColumnIndexMap.Remove(cellModel);
                    _columnCellsByIndex[cellModel.Column].Remove(cellModel);
                }
            }
        }

        public static List<CellViewModel> GetCellViewModelsForSheet(SheetViewModel sheet)
        {
            return GetCellModelsForSheet(sheet.SheetName).Select(x => CellViewModelFactory.Create(x, sheet)).ToList();
        }

        public static CellModel GetCellModel(string cellId)
        {
            return _cellsById.TryGetValue(cellId, out var cellModel) ? cellModel : new CellModel();
        }

        public static List<CellModel> GetCellModelsForSheet(string sheetName)
        {
            if (_cellsBySheetMap.TryGetValue(sheetName, out var cellDictionary))
            {
                return [.. cellDictionary.Values];
            }
            return [];
        }

        public static CellModel? GetCellModelForRow(int row) => _rowCellsByIndex.TryGetValue(row, out var list) ? list.FirstOrDefault() : null;

        public static CellModel? GetCellModelForColumn(int column) => _columnCellsByIndex.TryGetValue(column, out var list) ? list.FirstOrDefault() : null;

        private static void CellModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not CellModel model) return;
            if (e.PropertyName == nameof(CellModel.Row))
            {
                if (model.Column == 0)
                {
                    var previousRow = _rowModelToRowIndexMap[model];
                    _rowCellsByIndex[previousRow].Remove(model);
                    _rowModelToRowIndexMap[model] = model.Row;
                    AddCellToRowCellsByIndexMap(model);
                }
            }
            else if (e.PropertyName == nameof(CellModel.Column))
            {
                if (model.Row == 0)
                {
                    var previousColumn = _columnModelToColumnIndexMap[model];
                    _columnCellsByIndex[previousColumn].Remove(model);
                    _columnModelToColumnIndexMap[model] = model.Column;
                    AddCellToColumnCellsByIndexMap(model);
                }
            }

            CellLoader.SaveCell(model);
        }
    }
}
