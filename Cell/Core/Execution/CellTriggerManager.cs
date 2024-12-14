using Cell.Core.Common;
using Cell.Model;
using Cell.ViewModel.Application;
using Cell.Core.Execution.Functions;
using Cell.Core.Data.Tracker;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cell.Core.Execution
{
    /// <summary>
    /// Responsible for executing the trigger function of a cell when the value changes.
    /// </summary>
    public class CellTriggerManager
    {
        private readonly Dictionary<string, CellModel> _cellsBeingEdited = [];
        private readonly CellTracker _cellTracker;
        private readonly FunctionTracker _functionTracker;
        private readonly Logger _logger;
        private readonly UserCollectionTracker _userCollectionTracker;
        private readonly DialogFactoryBase _dialogFactoryForTriggers;

        /// <summary>
        /// Creates a new instance of the <see cref="CellTriggerManager"/> class.
        /// </summary>
        /// <param name="cellTracker">The cell tracker to monitor the cells within.</param>
        /// <param name="functionTracker">Used to get trigger functions from.</param>
        /// <param name="userCollectionTracker">The collection loader to provide to triggers context.</param>
        /// <param name="dialogFactoryForTriggers">The dialog factory used by trigger functions to show dialogs.</param>
        /// <param name="logger">The logger to log messages to.</param>
        public CellTriggerManager(CellTracker cellTracker, FunctionTracker functionTracker, UserCollectionTracker userCollectionTracker, DialogFactoryBase dialogFactoryForTriggers, Logger logger)
        {
            _userCollectionTracker = userCollectionTracker;
            _cellTracker = cellTracker;
            _dialogFactoryForTriggers = dialogFactoryForTriggers;
            _functionTracker = functionTracker;
            _logger = logger;
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
            CellTriggeredHandler(cell, editContext);
            _cellsBeingEdited.Remove(cell.ID);
        }

        internal IEnumerable<CellModel> GetCellsThatUseTriggerFunction(CellFunctionModel model)
        {
            return _cellTracker.AllCells.Where(x => x.TriggerFunctionName == model.Name);
        }

        private void CellTriggeredHandler(CellModel cell, EditContext editContext)
        {
            if (!_functionTracker.TryGetCellFunction("void", cell.TriggerFunctionName, out var triggerFunction)) return;
            var context = new Context(_cellTracker, _userCollectionTracker, _dialogFactoryForTriggers, cell)
            {
                E = editContext
            };
            var result = triggerFunction.Run(context);
            if (result.WasSuccess) return;
            _logger.Log($"Error: Trigger function {cell.TriggerFunctionName} has the following error '{result.ExecutionResult ?? "Error message is null"}'");
        }
    }
}
