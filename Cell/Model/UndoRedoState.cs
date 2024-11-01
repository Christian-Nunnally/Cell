
namespace Cell.Model
{
    public class UndoRedoState
    {
        public List<CellModel> CellsToRestore { get; set; } = [];
        public List<CellModel> CellsToAdd { get; set; } = [];
        public List<CellModel> CellsToRemove { get; set; } = [];

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Changed: {CellsToRestore.Count}, Removed: {CellsToAdd.Count}, Added: {CellsToRemove.Count}";
        }
    }
}
