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
            _cellTracker.CellAdded += StartMonitoringCell;
            _cellTracker.CellRemoved += StopMonitoringCell;
            foreach (var cell in _cellTracker.AllCells)
            {
                // TODO: Only monitor cells that a trigger function is set on.
                StartMonitoringCell(cell);
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

        public void StartMonitoringCell(CellModel model)
        {
            model.PropertyChanged += CellModelPropertyChanged;
            _cellModelToCurrentTextValueMap.Add(model, model.Text);
        }

        public void StopMonitoringCell(CellModel model)
        {
            model.PropertyChanged -= CellModelPropertyChanged;
            _cellModelToCurrentTextValueMap.Remove(model);
        }

        private void CellModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(CellModel.Text)) return;
            var cell = (CellModel)sender!;
            var oldValue = _cellModelToCurrentTextValueMap[cell];
            CellTriggered(cell, new EditContext(nameof(CellModel.Text), oldValue, cell.Text));
            _cellModelToCurrentTextValueMap[cell] = cell.Text;
        }
    }
}
