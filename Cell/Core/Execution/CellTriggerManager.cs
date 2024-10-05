using Cell.Common;
using Cell.Data;
using Cell.Model;
using Cell.Persistence;
using System.ComponentModel;

namespace Cell.Execution
{
    /// <summary>
    /// Responsible for executing the trigger function of a cell when the value changes.
    /// </summary>
    public class CellTriggerManager
    {
        private readonly Dictionary<CellModel, string> _cellModelToCurrentTextValueMap = [];
        private readonly Dictionary<string, CellModel> _cellsBeingEdited = [];
        private readonly CellTracker _cellTracker;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly UserCollectionLoader _userCollectionLoader;
        /// <summary>
        /// Creates a new instance of the <see cref="CellTriggerManager"/> class.
        /// </summary>
        /// <param name="cellTracker">The cell tracker to monitor the cells within.</param>
        /// <param name="pluginFunctionLoader">The plugin function loader to get trigger functions from.</param>
        /// <param name="userCollectionLoader">The collection loader to provide to triggers context.</param>
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

        /// <summary>
        /// Signals that a cells trigger function should be executed with the given context.
        /// </summary>
        /// <param name="cell">The triggering cell.</param>
        /// <param name="editContext">The triggers context.</param>
        public void CellTriggered(CellModel cell, EditContext editContext)
        {
            if (string.IsNullOrWhiteSpace(cell.TriggerFunctionName)) return;
            if (_cellsBeingEdited.ContainsKey(cell.ID)) return;
            _cellsBeingEdited.Add(cell.ID, cell);
            CellTriggeredHandler(cell);
            _cellsBeingEdited.Remove(cell.ID);
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

        private void CellTriggeredHandler(CellModel cell)
        {
            if (!_pluginFunctionLoader.TryGetFunction("void", cell.TriggerFunctionName, out var triggerFunction)) return;
            var context = new Context(_cellTracker, _userCollectionLoader, cell.Index);
            var result = triggerFunction.Run(context, cell);
            if (result.WasSuccess) return;
            Logger.Instance.Log($"Error: Trigger function {cell.TriggerFunctionName} has the following error '{result.ExecutionResult ?? "Error message is null"}'");
        }

        private void StartMonitoringCellForTriggerFunction(CellModel model)
        {
            model.PropertyChanged += CellModelPropertyChanged;
            if (!string.IsNullOrWhiteSpace(model.TriggerFunctionName))
            {
                _cellModelToCurrentTextValueMap.Add(model, model.Text);
            }
        }

        private void StopMonitoringCellForTriggerFunction(CellModel model)
        {
            model.PropertyChanged -= CellModelPropertyChanged;
            _cellModelToCurrentTextValueMap.Remove(model);
        }
    }
}
