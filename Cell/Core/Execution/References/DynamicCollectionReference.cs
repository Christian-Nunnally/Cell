using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Execution;

namespace Cell.Execution.References
{
    public class DynamicCollectionReference : ICollectionReference
    {
        private readonly CellFunction _calculateCollectionNameFunction;
        public DynamicCollectionReference(CellFunction calculateCollectionNameFunction)
        {
            _calculateCollectionNameFunction = calculateCollectionNameFunction;
            _calculateCollectionNameFunction.DependenciesChanged += CalculateCollectionNameFunctionDependenciesChanged;
        }

        public event Action? LocationsThatWillInvalidateCollectionNameForCellHaveChanged;

        public string GetCollectionName(CellModel cell, Context pluginFunctionRunContext)
        {
            pluginFunctionRunContext.Cell = cell;
            var result = _calculateCollectionNameFunction.Run(pluginFunctionRunContext, cell);
            if (result.WasSuccess && result.ReturnedObject is not null) return result.ReturnedObject.ToString() ?? "";
            Logger.Instance.Log($"Error calculating collection name from function {_calculateCollectionNameFunction}: {result.ExecutionResult}");
            return string.Empty;
        }

        public IEnumerable<string> GetLocationsThatWillInvalidateCollectionNameForCell(CellModel cell)
        {
            return _calculateCollectionNameFunction.LocationDependencies.SelectMany(x => x.ResolveLocations(cell.Location));
        }

        /// <summary>
        /// Gets a string that represents the name of the collection this reference is currently referring to. This might not be a collection name if the reference requires a cell to resolve.
        /// </summary>
        /// <returns>The name the reference is referring to, or some other representation if it requires a cell to resolve.</returns>
        public string ResolveUserFriendlyCellAgnosticName()
        {
            return _calculateCollectionNameFunction.Model.Code;
        }

        /// <summary>
        /// Gets a string that represents the name of the collection this reference is currently referring to.
        /// </summary>
        /// <param name="cell">The cell to use to resolve the reference.</param>
        /// <returns>The name the reference is referring to.</returns>
        public string ResolveUserFriendlyNameForCell(CellModel cell)
        {
            return string.Join(',', GetLocationsThatWillInvalidateCollectionNameForCell(cell));
        }

        private void CalculateCollectionNameFunctionDependenciesChanged(CellFunction function)
        {
            LocationsThatWillInvalidateCollectionNameForCellHaveChanged?.Invoke();
        }
    }
}
