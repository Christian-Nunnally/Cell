using Cell.Common;
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
        /// <summary>
        /// Creates a new instance of <see cref="CellPopulateSubscriber"/>.
        /// </summary>
        /// <param name="cell">The cell to populate when this subscriber is triggered.</param>
        /// <param name="cellTracker">The cell tracker used in the context when running populate.</param>
        /// <param name="userCollectionLoader">The user collection loader used in the context when running populate.</param>
        /// <param name="pluginFunctionLoader">The function loader to load the populate function from.</param>
        public CellPopulateSubscriber(CellModel cell, CellTracker cellTracker, UserCollectionLoader userCollectionLoader, PluginFunctionLoader pluginFunctionLoader)
        {
            _cell = cell;
            _cellTracker = cellTracker;
            _userCollectionLoader = userCollectionLoader;
            _pluginFunctionLoader = pluginFunctionLoader;
        }

        /// <summary>
        /// The action that is performed when this ISubscriber triggers.
        /// </summary>
        public void Action()
        {
            RunPopulateForSubscriber(_cell);
        }

        /// <summary>
        /// Provides a user friendly string version of this subscriber.
        /// </summary>
        /// <returns>The user friendly string version of this subscriber.</returns>
        public override string ToString() => $"Populate subscriber for {_cell.Location.UserFriendlyLocationString}";

        private CompileResult RunPopulate(CellModel subscriber)
        {
            var pluginContext = new Context(_cellTracker, _userCollectionLoader, subscriber);
            if (!_pluginFunctionLoader.TryGetCellFunction("object", subscriber.PopulateFunctionName, out var populateFunction)) return new CompileResult { WasSuccess = false, ExecutionResult = "Populate function not found" };
            var result = populateFunction.Run(pluginContext, subscriber);
            if (!result.WasSuccess) return result;
            var returnedObject = result.ReturnedObject;
            if (returnedObject is not null) result.ExecutionResult = returnedObject.ToString() ?? "";
            return result;
        }

        private void RunPopulateForSubscriber(CellModel subscriber)
        {
            var result = RunPopulate(subscriber);
            if (result.WasSuccess)
            {
                if (subscriber.CellType.IsCollection())
                {
                    if (result.ReturnedObject != null) subscriber.PopulateResult = result.ReturnedObject;
                }
                else
                {
                    if (result.ExecutionResult != null) subscriber.Text = result.ExecutionResult;
                }
            }
            else
            {
                subscriber.Text = result.ExecutionResult ?? "Execution result was null";
            }
        }
    }
}
