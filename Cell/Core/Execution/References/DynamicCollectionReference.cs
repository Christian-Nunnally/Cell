using Cell.Core.Common;
using Cell.Core.Execution.Functions;
using Cell.Model;

namespace Cell.Core.Execution.References
{
    /// <summary>
    /// Represents a reference to a collection that is calculated dynamically.
    /// </summary>
    public class DynamicCollectionReference : ICollectionReference
    {
        private readonly CellFunction _calculateCollectionNameFunction;
        /// <summary>
        /// Creates a new instance of <see cref="DynamicCollectionReference"/>.
        /// </summary>
        /// <param name="calculateCollectionNameFunction">The function that returns the collection this reference referrs to.</param>
        public DynamicCollectionReference(CellFunction calculateCollectionNameFunction)
        {
            _calculateCollectionNameFunction = calculateCollectionNameFunction;
            _calculateCollectionNameFunction.DependenciesChanged += CalculateCollectionNameFunctionDependenciesChanged;
        }

        /// <summary>
        /// Occurs when values have changes at locations that will result in the function returning a different collection name.
        /// </summary>
        public event Action? LocationsThatWillInvalidateCollectionNameForCellHaveChanged;

        /// <summary>
        /// Gets the name of the collection this reference is currently referring to.
        /// </summary>
        /// <param name="cell">The cell used to resolve the reference.</param>
        /// <param name="pluginFunctionRunContext">The context used when running the function.</param>
        /// <returns>The collection name this reference is currently referring to for that cell.</returns>
        public string GetCollectionName(CellModel cell, Context pluginFunctionRunContext)
        {
            pluginFunctionRunContext.Cell = cell;
            var result = _calculateCollectionNameFunction.Run(pluginFunctionRunContext, cell);
            if (result.WasSuccess && result.ReturnedObject is not null) return result.ReturnedObject.ToString() ?? "";
            Logger.Instance.Log($"Error calculating collection name from function {_calculateCollectionNameFunction}: {result.ExecutionResult}");
            return string.Empty;
        }

        /// <summary>
        /// Gets the locations that will invalidate the collection name for a particular cell.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns>The list of location this specific cell should watch for changes on, and if they do change that cell needs to recalculate this collection dependency.</returns>
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
