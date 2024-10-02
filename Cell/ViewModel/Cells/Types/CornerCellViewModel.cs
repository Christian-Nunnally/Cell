using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// Represents the cell in the top left corner of the sheet, which is a unique cell because it's part of the row and column headers.
    /// </summary>
    public class CornerCellViewModel : CellViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="CornerCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public CornerCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
        }
    }
}
