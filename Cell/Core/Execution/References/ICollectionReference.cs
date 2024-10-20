using Cell.Core.Execution.Functions;
using Cell.Model;

namespace Cell.Core.Execution.References
{
    /// <summary>
    /// Interface for references to collections from cells.
    /// </summary>
    public interface ICollectionReference : IReferenceFromCell
    {
        /// <summary>
        /// Occurs when values have changes at locations that will result in the function returning a different collection name.
        /// </summary>
        event Action? LocationsThatWillInvalidateCollectionNameForCellHaveChanged;

        /// <summary>
        /// Gets the name of the collection this reference is currently referring to.
        /// </summary>
        /// <param name="cell">The cell used to resolve the reference.</param>
        /// <param name="pluginFunctionRunContext">The context used when running the function.</param>
        /// <returns>The collection name this reference is currently referring to for that cell.</returns>
        string GetCollectionName(CellModel cell, Context pluginFunctionRunContext);

        /// <summary>
        /// Gets the locations that will invalidate the collection name for a particular cell.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns>The list of location this specific cell should watch for changes on, and if they do change that cell needs to recalculate this collection dependency.</returns>
        IEnumerable<string> GetLocationsThatWillInvalidateCollectionNameForCell(CellModel cell);
    }
}
