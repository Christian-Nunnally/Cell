using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// Represents a cell that displays a simple label that is not editable from the sheet.
    /// </summary>
    public class LabelCellViewModel : CellViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="LabelCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public LabelCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
        }
    }
}
