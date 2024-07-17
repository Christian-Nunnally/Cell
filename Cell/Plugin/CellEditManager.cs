using Cell.Model;
using Cell.ViewModel;

namespace Cell.Plugin
{
    /// <summary>
    /// Responsible for executing the OnEdit function of a cell when the value changes.
    /// </summary>
    public static class CellEditManager
    {
        private readonly static Dictionary<string, CellModel> _cellsBeingEdited = [];

        public static void StartMonitoringCellForEdits(CellModel model)
        {
            model.OnCellEdited += CellEdited;
        }

        public static void StopMonitoringCellForEdits(CellModel model)
        {
            model.OnCellEdited -= CellEdited;
        }

        public static void CellEdited(CellModel cell, EditContext editContext)
        {
            if (string.IsNullOrWhiteSpace(cell.TriggerFunctionName) || _cellsBeingEdited.ContainsKey(cell.ID)) return;
            _cellsBeingEdited.Add(cell.ID, cell);
            var result = DynamicCellPluginExecutor.RunTrigger(new PluginContext(ApplicationViewModel.Instance), cell);
            if (!result.Success)
            {
                cell.Text = result.Result;
            }
            _cellsBeingEdited.Remove(cell.ID);
        }
    }
}
