using Cell.Model;
using Cell.ViewModel;

namespace Cell.Plugin
{
    /// <summary>
    /// Responsible for executing the OnEdit function of a cell when the value changes.
    /// </summary>
    public static class CellTriggerManager
    {
        private readonly static Dictionary<string, CellModel> _cellsBeingEdited = [];

        public static void StartMonitoringCell(CellModel model)
        {
            model.CellTriggered += CellTriggered;
        }

        public static void StopMonitoringCell(CellModel model)
        {
            model.CellTriggered -= CellTriggered;
        }

        public static void CellTriggered(CellModel cell, EditContext editContext)
        {
            if (string.IsNullOrWhiteSpace(cell.TriggerFunctionName) || _cellsBeingEdited.ContainsKey(cell.ID)) return;
            _cellsBeingEdited.Add(cell.ID, cell);
            var result = DynamicCellPluginExecutor.RunTrigger(new PluginContext(ApplicationViewModel.Instance, cell.Index) { E = editContext }, cell);
            if (!result.Success)
            {
                cell.ErrorText = result.Result ?? "error message is null";
            }
            _cellsBeingEdited.Remove(cell.ID);
        }
    }
}
