using Cell.Model;

namespace Cell.Core.Execution.References
{
    /// <summary>
    /// Represents a reference from a cell to another object, like a cell or a collection.
    /// </summary>
    public interface IReferenceFromCell
    {
        /// <summary>
        /// Gets a string that represents the name of the collection this reference is currently referring to. This might not be a collection name if the reference requires a cell to resolve.
        /// </summary>
        /// <returns>The name the reference is referring to, or some other representation if it requires a cell to resolve.</returns>
        public string ResolveUserFriendlyCellAgnosticName();

        /// <summary>
        /// Gets a string that represents the name of the collection this reference is currently referring to.
        /// </summary>
        /// <param name="cell">The cell to use to resolve the reference.</param>
        /// <returns>The name the reference is referring to.</returns>
        public string ResolveUserFriendlyNameForCell(CellModel cell);
    }
}
