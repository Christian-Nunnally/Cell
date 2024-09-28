using Cell.Model;

namespace Cell.Execution.References
{
    /// <summary>
    /// Represents a reference to a collection that does not change.
    /// </summary>
    public class ConstantCollectionReference : ICollectionReference
    {
        public readonly static ConstantCollectionReference Null = new(string.Empty);

        public readonly string ConstantCollectionName;
        public ConstantCollectionReference(string collectionName)
        {
            ConstantCollectionName = collectionName;
        }

        public event Action? LocationsThatWillInvalidateCollectionNameForCellHaveChanged { add { } remove { } }

        public string GetCollectionName(CellModel cell, Context pluginFunctionRunContext) => ConstantCollectionName;

        public IEnumerable<string> GetLocationsThatWillInvalidateCollectionNameForCell(CellModel cell) => [];

        public string ResolveUserFriendlyCellAgnosticName() => ConstantCollectionName;

        public string ResolveUserFriendlyNameForCell(CellModel cell) => ConstantCollectionName;
    }
}
