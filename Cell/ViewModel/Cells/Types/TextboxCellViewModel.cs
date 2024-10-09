using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// A cell view model that displays a textbox.
    /// </summary>
    public class TextboxCellViewModel : CellViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="TextboxCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public TextboxCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
        }

        /// <summary>
        /// Gets the number of selected cells for the view so the textbox knows whether to focus or not.
        /// </summary>
        /// <returns>The number of cells selected by the owning sheets selector.</returns>
        public int GetNumberOfSelectedCells() => _sheetViewModel.CellSelector.SelectedCells.Count;
    }
}
