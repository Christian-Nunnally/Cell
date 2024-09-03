using Cell.Model;
using Cell.Persistence;
using Cell.ViewModel.Application;

namespace Cell.Execution
{
    /// <summary>
    /// Responsible for executing the OnEdit function of a cell when the value changes.
    /// </summary>
    public class CellTriggerManager
    {
        private readonly Dictionary<string, CellModel> _cellsBeingEdited = [];
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        public CellTriggerManager(PluginFunctionLoader pluginFunctionLoader)
        {
            _pluginFunctionLoader = pluginFunctionLoader;
        }

        public void CellTriggered(CellModel cell, EditContext editContext)
        {
            if (string.IsNullOrWhiteSpace(cell.TriggerFunctionName) || _cellsBeingEdited.ContainsKey(cell.ID)) return;
            _cellsBeingEdited.Add(cell.ID, cell);
            var result = DynamicCellPluginExecutor.RunTrigger(_pluginFunctionLoader, new PluginContext(ApplicationViewModel.Instance, cell.Index) { E = editContext }, cell);
            if (!result.Success)
            {
                cell.ErrorText = result.Result ?? "error message is null";
            }
            _cellsBeingEdited.Remove(cell.ID);
        }

        public void StartMonitoringCell(CellModel model)
        {
            model.CellTriggered += CellTriggered;
        }

        public void StopMonitoringCell(CellModel model)
        {
            model.CellTriggered -= CellTriggered;
        }
    }
}
