using Cell.Core.Execution.Functions;
using Cell.Model;

namespace Cell.Core.Execution.References
{
    /// <summary>
    /// Represents a reference to a collection that does not change.
    /// </summary>
    public class ConstantCollectionReference : ICollectionReference
    {
        /// <summary>
        /// A null reference that can be used as a placeholder.
        /// </summary>
        public readonly static ConstantCollectionReference Null = new(string.Empty);

        /// <summary>
        /// The name of the collection this reference is referring to.
        /// </summary>
        public readonly string ConstantCollectionName;
        /// <summary>
        /// Creates a new instance of the <see cref="ConstantCollectionReference"/> class.
        /// </summary>
        /// <param name="collectionName">The name of the collection this reference is referring to.</param>
        public ConstantCollectionReference(string collectionName)
        {
            ConstantCollectionName = collectionName;
        }

        /// <summary>
        /// Will always be null because this reference does not change.
        /// </summary>
        public event Action? LocationsThatWillInvalidateCollectionNameForCellHaveChanged { add { } remove { } }

        /// <summary>
        /// Gets the name of the collection this reference is currently referring to.
        /// </summary>
        /// <param name="cell">The cell used to help resolve relative references.</param>
        /// <param name="pluginFunctionRunContext">The context used if a function needs to be run to resolve the name.</param>
        /// <returns>The name of the referenced collection.</returns>
        public string GetCollectionName(CellModel cell, IContext pluginFunctionRunContext) => ConstantCollectionName;

        /// <summary>
        /// Will always return an empty collection because this reference does not change.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns>An empty list for this subtype.</returns>
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
