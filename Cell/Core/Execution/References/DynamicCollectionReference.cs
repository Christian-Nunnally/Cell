using Cell.Common;
using Cell.Model;
using Cell.ViewModel.Execution;

namespace Cell.Execution.References
{
    public class DynamicCollectionReference : ICollectionReference
    {
        private readonly PluginFunction _calculateCollectionNameFunction;
        public DynamicCollectionReference(PluginFunction calculateCollectionNameFunction)
        {
            _calculateCollectionNameFunction = calculateCollectionNameFunction;
            _calculateCollectionNameFunction.DependenciesChanged += CalculateCollectionNameFunctionDependenciesChanged;
        }

        public event Action? LocationsThatWillInvalidateCollectionNameForCellHaveChanged;

        public string GetCollectionName(CellModel cell, PluginContext pluginFunctionRunContext)
        {
            pluginFunctionRunContext.Cell = cell;
            var result = _calculateCollectionNameFunction.Run(pluginFunctionRunContext, cell);
            if (result.WasSuccess && result.ReturnedObject is not null) return result.ReturnedObject.ToString() ?? "";
            Logger.Instance.Log($"Error calculating collection name from function {_calculateCollectionNameFunction}: {result.ExecutionResult}");
            return string.Empty;
        }

        public IEnumerable<string> GetLocationsThatWillInvalidateCollectionNameForCell(CellModel cell)
        {
            return _calculateCollectionNameFunction.LocationDependencies.SelectMany(x => x.ResolveLocations(cell));
        }

        public string ResolveUserFriendlyCellAgnosticName()
        {
            return _calculateCollectionNameFunction.Model.Code;
        }

        public string ResolveUserFriendlyNameForCell(CellModel cell)
        {
            return string.Join(',', GetLocationsThatWillInvalidateCollectionNameForCell(cell));
        }

        private void CalculateCollectionNameFunctionDependenciesChanged(PluginFunction function)
        {
            LocationsThatWillInvalidateCollectionNameForCellHaveChanged?.Invoke();
        }
    }
}
