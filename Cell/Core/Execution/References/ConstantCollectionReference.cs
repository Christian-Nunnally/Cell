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

        /// <summary>
        /// Gets a string that represents the name of the collection this reference is currently referring to. This might not be a collection name if the reference requires a cell to resolve.
        /// </summary>
        /// <returns>The name the reference is referring to, or some other representation if it requires a cell to resolve.</returns>
        public string ResolveUserFriendlyCellAgnosticName() => ConstantCollectionName;

        /// <summary>
        /// Gets a string that represents the name of the collection this reference is currently referring to.
        /// </summary>
        /// <param name="cell">The cell to use to resolve the reference.</param>
        /// <returns>The name the reference is referring to.</returns>
        public string ResolveUserFriendlyNameForCell(CellModel cell) => ConstantCollectionName;
    }
}
