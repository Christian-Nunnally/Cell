using Cell.Model;

namespace Cell.Execution.References
{
    public interface ICollectionReference
    {
        event Action? LocationsThatWillInvalidateCollectionNameForCellHaveChanged;

        string GetCollectionName(CellModel cell, PluginContext pluginFunctionRunContext);

        IEnumerable<string> GetLocationsThatWillInvalidateCollectionNameForCell(CellModel cell);
    }
}
