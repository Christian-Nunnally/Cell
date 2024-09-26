using Cell.Data;
using Cell.Model;
using Cell.Persistence;

namespace Cell.Execution
{
    /// <summary>
    /// When something this subscriber subscribes to publishes, this class runs the given cells populate function and sets it's text to the result.
    /// </summary>
    public class CellPopulateSubscriber : ISubscriber
    {
        private readonly CellModel _cell;
        private readonly CellTracker _cellTracker;
        private readonly PluginFunctionLoader _pluginFunctionLoader;
        private readonly UserCollectionLoader _userCollectionLoader;
        public CellPopulateSubscriber(CellModel cell, CellTracker cellTracker, UserCollectionLoader userCollectionLoader, PluginFunctionLoader pluginFunctionLoader)
        {
            _cell = cell;
            _cellTracker = cellTracker;
            _userCollectionLoader = userCollectionLoader;
            _pluginFunctionLoader = pluginFunctionLoader;
        }

        public void Action()
        {
            RunPopulateForSubscriber(_cell);
        }

        private void RunPopulateForSubscriber(CellModel subscriber)
        {
            // You can remove this once cells are properly unsubscribed from this when PopulateFunctionName is set to "";
            if (subscriber.PopulateFunctionName == "") return;

            var pluginContext = new PluginContext(_cellTracker, _userCollectionLoader, subscriber);
            var result = DynamicCellPluginExecutor.RunPopulate(_pluginFunctionLoader, pluginContext, subscriber);
            if (result.WasSuccess)
            {
                if (result.ExecutionResult != null) subscriber.Text = result.ExecutionResult;
                subscriber.ErrorText = string.Empty;
            }
            else
            {
                if (result.ExecutionResult != null) subscriber.Text = result.ExecutionResult;// "Error";
                if (result.ExecutionResult != null) subscriber.ErrorText = result.ExecutionResult;
            }
        }

        public override string ToString() => $"Populate subscriber for {_cell.UserFriendlyCellName}";
    }
}
