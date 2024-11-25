
namespace Cell.Model
{
    /// <summary>
    /// Represents the state of the cells before a change recorded by the undo/redo system.
    /// </summary>
    public class UndoRedoState
    {
        /// <summary>
        /// List of cells that were changed.
        /// </summary>
        public List<CellModel> CellsToRestore { get; set; } = [];

        /// <summary>
        /// List of cells that were removed.
        /// </summary>
        public List<CellModel> CellsToAdd { get; set; } = [];

        /// <summary>
        /// List of cells that were added.
        /// </summary>
        public List<CellModel> CellsToRemove { get; set; } = [];

        /// <summary>
        /// List of cells that were added.
        /// </summary>
        public List<CellFunctionModel> FunctionsToRestore { get; set; } = [];

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Changed: {CellsToRestore.Count}, Removed: {CellsToAdd.Count}, Added: {CellsToRemove.Count}";
        }
    }
}
