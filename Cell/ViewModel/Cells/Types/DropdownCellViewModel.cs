using Cell.Model;

namespace Cell.ViewModel.Cells.Types
{
    /// <summary>
    /// A cell that displays a dropdown list of items.
    /// 
    /// The user can select an item and it will be displayed in the cell.
    /// </summary>
    public class DropdownCellViewModel : CollectionCellViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="DropdownCellViewModel"/>.
        /// </summary>
        /// <param name="model">The underlying model for this cell.</param>
        /// <param name="sheet">The sheet this cell is visible on.</param>
        public DropdownCellViewModel(CellModel model, SheetViewModel sheet) : base(model, sheet)
        {
        }

        public string SelectedItem { get; set; }

        public override string Text { get => SelectedItem; set => base.Text = value; }
    }
}
