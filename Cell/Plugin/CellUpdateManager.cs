using Cell.Model;
using Cell.ViewModel;

namespace Cell.Plugin
{
    /// <summary>
    /// Responsible for running other cells GetText functions when cells or collections they reference are updated.
    /// </summary>
    internal class CellUpdateManager
    {
        private readonly static Dictionary<string, CellModel> _cellsBeingUpdated = [];
        private readonly static Dictionary<string, List<CellModel>> _cellsToNotifyOnUpdates = [];
        private readonly static Dictionary<CellModel, List<string>> _subcriptionsMadeByCells = [];

        public static void StartMonitoringCellForUpdates(CellModel model)
        {
            model.AfterCellEdited += NotifyCellValueUpdated;
        }

        public static void StopMonitoringCellForUpdates(CellModel model)
        {
            model.AfterCellEdited -= NotifyCellValueUpdated;
        }

        public static void SubscribeToCellValueUpdates(CellModel subscriber, string sheet, int row, int column)
        {
            var key = GetUnqiueLocationString(sheet, row, column);
            if (_cellsToNotifyOnUpdates.TryGetValue(key, out var subscribers) && !subscribers.Contains(subscriber)) subscribers.Add(subscriber);
            else _cellsToNotifyOnUpdates.Add(key, [subscriber]);
            if (_subcriptionsMadeByCells.TryGetValue(subscriber, out var subscriptions)) subscriptions.Add(key);
            else _subcriptionsMadeByCells.Add(subscriber, [key]);
        }

        public static void UnsubscribeFromCellValueUpdates(CellModel model)
        {
            if (!_subcriptionsMadeByCells.TryGetValue(model, out var subscriptions)) return;
            foreach (var subscription in subscriptions)
            {
                if (!_cellsToNotifyOnUpdates.TryGetValue(subscription, out var subscribers)) continue;
                subscribers.Remove(model);
            }
        }

        public static void NotifyCellValueUpdated(CellModel cell)
        {
            if (_cellsBeingUpdated.ContainsKey(cell.ID)) return;
            _cellsBeingUpdated.Add(cell.ID, cell);
            var key = GetUnqiueLocationString(cell.SheetName, cell.Row, cell.Column);
            if (_cellsToNotifyOnUpdates.TryGetValue(key, out var subscribers))
            {
                foreach (var subscriber in subscribers)
                {
                    var result = DynamicCellPluginExecutor.CompileAndRunPopulate(new PluginContext(ApplicationViewModel.Instance), subscriber);
                    subscriber.Text = result.Result;
                    subscriber.Value = result.Result;
                }
            }
            _cellsBeingUpdated.Remove(cell.ID);
        }

        private static string GetUnqiueLocationString(string sheet, int row, int column) => $"{sheet}_{row}_{column}";
    }
}
