using Cell.Model;
using Cell.ViewModel.Application;
using Cell.Core.Execution.Functions;
using Cell.Core.Data.Tracker;

namespace Cell.Core.Execution
{
    /// <summary>
    /// When something this subscriber subscribes to publishes, this class runs the given cells populate function and sets it's text to the result.
    /// </summary>
    public class CellPopulateSubscriber : ISubscriber
    {
        private readonly CellModel _cell;
        private readonly CellTracker _cellTracker;
        private readonly FunctionTracker _functionTracker;
        private readonly UserCollectionTracker _userCollectionTracker;
        /// <summary>
        /// Creates a new instance of <see cref="CellPopulateSubscriber"/>.
        /// </summary>
        /// <param name="cell">The cell to populate when this subscriber is triggered.</param>
        /// <param name="cellTracker">The cell tracker used in the context when running populate.</param>
        /// <param name="userCollectionTracker">The user collection tracker used in the context when running populate.</param>
        /// <param name="functionTracker">The function tracker to get the populate function from.</param>
        public CellPopulateSubscriber(CellModel cell, CellTracker cellTracker, UserCollectionTracker userCollectionTracker, FunctionTracker functionTracker)
        {
            _cell = cell;
            _cellTracker = cellTracker;
            _userCollectionTracker = userCollectionTracker;
            _functionTracker = functionTracker;
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
            var pluginContext = new Context(_cellTracker, _userCollectionTracker, new DialogFactory(), subscriber);
            if (!_functionTracker.TryGetCellFunction("object", subscriber.PopulateFunctionName, out var populateFunction)) return new CompileResult { WasSuccess = false, ExecutionResult = "Populate function not found" };
            var result = populateFunction.Run(pluginContext);
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
