using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using System.ComponentModel;

namespace Cell.Execution
{
    /// <summary>
    /// Responsible for executing the OnEdit function of a cell when the value changes.
    /// </summary>
    public class CellTriggerManager
    {
        private readonly Dictionary<CellModel, string> _cellModelToCurrentTextValueMap = [];
        private readonly Dictionary<string, CellModel> _cellsBeingEdited = [];
        private readonly CellTracker _cellTracker;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly UserCollectionLoader _userCollectionLoader;
        public CellTriggerManager(CellTracker cellTracker, PluginFunctionLoader pluginFunctionLoader, UserCollectionLoader userCollectionLoader)
        {
            _userCollectionLoader = userCollectionLoader;
            _cellTracker = cellTracker;
            _cellTracker.CellAdded += StartMonitoringCellForTriggerFunction;
            _cellTracker.CellRemoved += StopMonitoringCellForTriggerFunction;
            foreach (var cell in _cellTracker.AllCells)
            {
                StartMonitoringCellForTriggerFunction(cell);
            }
            _pluginFunctionLoader = pluginFunctionLoader;
        }

        public void CellTriggered(CellModel cell, EditContext editContext)
        {
            if (string.IsNullOrWhiteSpace(cell.TriggerFunctionName) || _cellsBeingEdited.ContainsKey(cell.ID)) return;
            _cellsBeingEdited.Add(cell.ID, cell);
            var result = DynamicCellPluginExecutor.RunTrigger(_pluginFunctionLoader, new Context(_cellTracker, _userCollectionLoader, cell.Index) { E = editContext }, cell);
            if (!result.WasSuccess)
            {
                cell.ErrorText = result.ExecutionResult ?? "error message is null";
            }
            _cellsBeingEdited.Remove(cell.ID);
        }

        public void StartMonitoringCellForTriggerFunction(CellModel model)
        {
            model.PropertyChanged += CellModelPropertyChanged;
            if (!string.IsNullOrWhiteSpace(model.TriggerFunctionName))
            {
                _cellModelToCurrentTextValueMap.Add(model, model.Text);
            }
        }

        public void StopMonitoringCellForTriggerFunction(CellModel model)
        {
            model.PropertyChanged -= CellModelPropertyChanged;
            _cellModelToCurrentTextValueMap.Remove(model);
        }

        private void CellModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not CellModel cell) return;
            if (e.PropertyName == nameof(CellModel.Text))
            {
                if (_cellModelToCurrentTextValueMap.TryGetValue(cell, out var oldValue))
                {
                    CellTriggered(cell, new EditContext(nameof(CellModel.Text), oldValue, cell.Text));
                    _cellModelToCurrentTextValueMap[cell] = cell.Text;
                }
            }
            else if (e.PropertyName == nameof(CellModel.TriggerFunctionName))
            {
                if (cell.TriggerFunctionName == string.Empty)
                {
                    _cellModelToCurrentTextValueMap.Remove(cell);
                }
                else
                {
                    if (!_cellModelToCurrentTextValueMap.ContainsKey(cell))
                    {
                        _cellModelToCurrentTextValueMap.Add(cell, cell.Text);
                    }
                }
            }    
        }
    }
}
